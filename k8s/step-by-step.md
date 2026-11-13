me de um passo a passo de como:
- Iniciar o minikube (já tenho podman instalado)
- Criar um namespace para a minha aplicação
- Criar um deployment com 3 replicas para a aplicação, 1 POD para um banco de dados (no futuro, podemos evoluir para um statefulset com PV e PVC e addons do minikube), 1 POD para o elastic search, um service para o bd e um para o ES ( a aplicação é quem vai acessar os 2) 
- 1 Configmap para variaveis de ambiente e secrets para acesso ao BD e ES
- Probes (liveness e readiness)
- 1 Ingress NGINX roteando /api para a API
- Rolling update.

Quero aprender enquanto faço, embora já tenha feito no passado e entenda tudo teoricamente.
Então cada etapa deve ser incremental e verificável.
Já tenho a aplicação, o ES e o BD (postgres) rodando via docker compose e podman nesse momento

O ChatGPT disse:
Pesquisando na web
Vamos montar isso como um laboratório incremental de Kubernetes, usando Minikube + Podman, reaproveitando sua aplicação, seu PostgreSQL e seu Elasticsearch que já funcionam via Compose.

A ideia é não criar todos os manifests de uma vez. Vamos construir a infraestrutura em pequenas etapas, validar cada recurso com kubectl e só avançar quando soubermos o que está funcionando. Assim, você também vai aprender a diagnosticar os problemas que aparecem na prática.

1. O que vamos construir
Ingress NGINX

Entrada HTTP · /api

NAMESPACE: minha-app

Service da API

ClusterIP · porta da API

Pod 1

Pod 2

Pod 3

Deployment · 3 réplicas

PostgreSQL

1 Pod · Service

Elasticsearch

1 Pod · Service

ConfigMap + Secrets

Configuração não sensível, credenciais e parâmetros de conexão.

Os Services do banco e do Elasticsearch serão acessados pela API usando DNS interno do Kubernetes. Não precisamos expor esses dois serviços para fora do cluster.

Decisões para este laboratório
API: Deployment com 3 réplicas.

PostgreSQL: 1 réplica, inicialmente em Deployment, sem persistência Kubernetes.

Elasticsearch: 1 réplica, inicialmente em Deployment.

Rede: Services ClusterIP para API, banco e Elasticsearch.

Configuração: ConfigMap para valores não sensíveis; Secrets para credenciais.

Saúde: readiness e liveness probes na API; probes adequadas também para os serviços que suportarem isso.

Entrada: Ingress NGINX roteando /api para a API.

Atualizações: rolling update com possibilidade de rollback.

Uma ressalva importante: um Pod de banco ou Elasticsearch sem armazenamento persistente no Kubernetes pode perder seus dados ao ser recriado. Além disso, como você já tem esses serviços rodando no Compose, não vamos presumir que os dados ou volumes atuais serão migrados automaticamente.

2. Etapa zero — entender o ambiente atual
Antes de iniciar o Minikube, vamos identificar como o Compose está organizado. Isso evita um erro muito comum: tentar reutilizar no Kubernetes uma imagem ou hostname que só funciona dentro da rede do Compose.

Abra um terminal na máquina onde você já executa a aplicação.

Execute:

bash
podman --version
podman info
podman ps
podman compose ps
minikube version
kubectl version --client
Se podman compose ps não funcionar fora do diretório do projeto, entre na pasta onde está seu compose.yaml e repita o comando.

O que observar:

Qual é o nome da imagem da API?

O PostgreSQL está publicado em uma porta do host ou acessível somente pela rede Compose?

Qual porta o Elasticsearch utiliza? Normalmente, 9200 é a API HTTP.

Quais variáveis de ambiente a aplicação usa para conectar aos dois serviços?

A aplicação depende de algum arquivo de configuração ou volume?

Guarde esses dados. Vamos precisar deles para transformar a configuração atual em recursos Kubernetes.

Um detalhe importante sobre as imagens
O Minikube não compartilha automaticamente todas as imagens do Podman com o Kubernetes. Se sua API é construída localmente, teremos de disponibilizar essa imagem para o cluster, por exemplo, usando minikube image load ou uma estratégia de registro de imagens.

Veja a documentação oficial de 
Minikube com Podman . 

minikube

3. Etapa 1 — iniciar o Minikube
3.1. Escolher o driver
O comando depende do seu sistema operacional e de como o Podman está instalado.

Se estiver usando Linux com Podman rootless, a documentação recomenda containerd como runtime do Kubernetes:

bash
minikube start \
  --driver=podman \
  --container-runtime=containerd \
  --cpus=4 \
  --memory=6144 \
  --disk-size=30g
Esses valores são um ponto de partida, não uma garantia de que tudo caberá na sua máquina. Elasticsearch pode consumir bastante memória, portanto, ajuste conforme os recursos disponíveis.

Se estiver no macOS ou Windows, verifique primeiro se a máquina virtual do Podman está ativa:

bash
podman machine start
podman info
Nesse caso, podemos começar com:

bash
minikube start --driver=podman
O Minikube documenta diferenças entre Podman rootless, rootful e os sistemas que dependem de uma máquina virtual. Não force containerd sem considerar essas diferenças. 

minikube
+1

3.2. Validar o cluster
Quando o comando terminar, execute:

bash
minikube status
kubectl config current-context
kubectl get nodes -o wide
kubectl get pods -A
Você deve observar:

Minikube com os componentes principais em execução.

O contexto do kubectl apontando para minikube.

Um nó com status Ready.

Os Pods de infraestrutura sendo inicializados nos namespaces do sistema.

Para uma validação adicional:

bash
kubectl cluster-info
kubectl get events -A --sort-by=.lastTimestamp
Se algo estiver em Pending, ImagePullBackOff ou CrashLoopBackOff, não avance ainda. Vamos diagnosticar a causa antes de adicionar novos recursos.

3.3. Entenda o que acabou de acontecer
minikube start não iniciou sua aplicação, seu banco ou seu Elasticsearch do Compose. Ele criou um cluster Kubernetes local, com seu próprio plano de controle e ambiente de execução de Pods.

Por enquanto, não precisamos habilitar todos os addons. Vamos ativar o Ingress quando chegarmos à etapa de entrada HTTP.

4. Etapa 2 — criar o namespace da aplicação
Vamos chamar o namespace de minha-app. Você pode trocar esse nome pelo nome real do projeto.

Crie o diretório de trabalho:

bash
mkdir -p k8s
cd k8s
Crie namespace.yaml:

yaml
apiVersion: v1
kind: Namespace
metadata:
  name: minha-app
  labels:
    app.kubernetes.io/part-of: minha-app
Aplique o manifesto:

bash
kubectl apply -f namespace.yaml
Valide:

bash
kubectl get namespaces
kubectl describe namespace minha-app
Agora defina o namespace como padrão para os próximos comandos deste contexto:

bash
kubectl config set-context --current --namespace=minha-app
Confirme:

bash
kubectl config view --minify | grep namespace
kubectl get pods
O segundo comando provavelmente não mostrará Pods da aplicação ainda. Isso é esperado.

Conceito importante: o namespace organiza e isola logicamente os recursos; ele não cria, por si só, uma rede isolada ou uma política de segurança de rede. Também não impede que você crie acidentalmente recursos em outro namespace quando especifica -n ou --namespace explicitamente.

5. Etapa 3 — organizar os arquivos Kubernetes
Vamos manter os manifestos separados por responsabilidade, para que seja fácil entender o que cada arquivo cria.

Uma estrutura possível:

text
k8s/
├── namespace.yaml
├── configmap.yaml
├── secret.yaml
├── postgres.yaml
├── elasticsearch.yaml
├── api.yaml
└── ingress.yaml
Mais tarde, se fizer sentido, podemos migrar para Helm ou Kustomize. Por enquanto, YAML puro é ótimo para aprender.

6. Etapa 4 — ConfigMap e Secrets
Antes de criar os Pods, vamos definir como a aplicação receberá as configurações de conexão.

6.1. Criar o ConfigMap
O ConfigMap deve conter apenas configurações não sensíveis. Por exemplo, crie configmap.yaml:

yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: minha-app-config
  namespace: minha-app
data:
  APP_ENV: "development"
  DB_HOST: "postgres"
  DB_PORT: "5432"
  DB_NAME: "minha_app"
  ES_HOST: "elasticsearch"
  ES_PORT: "9200"
Os nomes postgres e elasticsearch serão os nomes dos Services que criaremos nas próximas etapas.

Aplique e valide:

bash
kubectl apply -f configmap.yaml
kubectl get configmap
kubectl describe configmap minha-app-config
Observe como o Kubernetes guarda os valores sem precisar incorporá-los diretamente à imagem da API.

6.2. Criar o Secret
Crie secret.yaml:

yaml
apiVersion: v1
kind: Secret
metadata:
  name: minha-app-secrets
  namespace: minha-app
type: Opaque
stringData:
  DB_USER: "SEU_USUARIO"
  DB_PASSWORD: "SUA_SENHA"
  ES_USERNAME: "SEU_USUARIO_ES"
  ES_PASSWORD: "SUA_SENHA_ES"
Substitua os valores pelos que correspondem ao seu ambiente. Se o Elasticsearch não exigir autenticação, não invente credenciais: adaptaremos a configuração ao seu modo de execução.

Aplique:

bash
kubectl apply -f secret.yaml
kubectl get secrets
Para verificar as chaves presentes, sem imprimir as credenciais:

bash
kubectl describe secret minha-app-secrets
Atenção: o objeto Secret não equivale a um cofre de segredos. A codificação Base64 não é criptografia, e um manifesto com credenciais reais não deve ser versionado em Git. Neste laboratório, use credenciais locais de desenvolvimento e mantenha o arquivo fora do controle de versão; futuramente, podemos adotar um gerenciador de segredos.

6.3. Como a API consumirá essas configurações
No Deployment, usaremos:

yaml
envFrom:
  - configMapRef:
      name: minha-app-config
  - secretRef:
      name: minha-app-secrets
Isso injeta as chaves como variáveis de ambiente nos containers.

Por que não colocar tudo no ConfigMap? Porque queremos separar claramente configuração de credenciais, mesmo que a aplicação receba ambas da mesma forma.

Validação desta etapa:

bash
kubectl get configmap,secret
Ainda não existe uma aplicação consumindo essas variáveis. Isso é proposital: estamos preparando as dependências primeiro.

7. Etapa 5 — preparar as imagens e implantar o PostgreSQL
Aqui existe uma decisão importante, pois você já tem um PostgreSQL rodando via Compose.

Não vamos executar um segundo banco com os mesmos dados sem decidir como eles serão separados. Existem duas estratégias:

Estratégia A — banco dentro do Minikube: criar um PostgreSQL Kubernetes de laboratório, com dados independentes e sem persistência durável.

Estratégia B — reutilizar o PostgreSQL do Compose: manter o banco atual fora do Kubernetes e configurar a API para alcançá-lo por um endereço acessível a partir do cluster.

Como seu objetivo é aprender Deployment, Service e DNS interno, recomendo a estratégia A para o laboratório, desde que você aceite usar um banco separado e descartável. Não remova o banco atual nem seus volumes.

7.1. Criar o Deployment do PostgreSQL
Crie postgres.yaml:

yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
  namespace: minha-app
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
        - name: postgres
          image: postgres:16
          ports:
            - name: postgres
              containerPort: 5432
          env:
            - name: POSTGRES_DB
              valueFrom:
                configMapKeyRef:
                  name: minha-app-config
                  key: DB_NAME
            - name: POSTGRES_USER
              valueFrom:
                secretKeyRef:
                  name: minha-app-secrets
                  key: DB_USER
            - name: POSTGRES_PASSWORD
              valueFrom:
                secretKeyRef:
                  name: minha-app-secrets
                  key: DB_PASSWORD
          readinessProbe:
            exec:
              command:
                - sh
                - -c
                - pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB"
            initialDelaySeconds: 5
            periodSeconds: 5
          livenessProbe:
            exec:
              command:
                - sh
                - -c
                - pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB"
            initialDelaySeconds: 20
            periodSeconds: 10
Este manifesto usa a imagem oficial do PostgreSQL 16 como exemplo. Ajuste a versão para a mesma versão principal do banco que você já utiliza, para evitar incompatibilidades.

A configuração pressupõe um banco novo. Se você reaproveitar um diretório de dados existente, as variáveis POSTGRES_DB, POSTGRES_USER e POSTGRES_PASSWORD não alteram automaticamente as credenciais e os bancos já inicializados.

7.2. Criar o Service do PostgreSQL
Adicione ao final do mesmo arquivo, separado por ---:

yaml
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: minha-app
spec:
  type: ClusterIP
  selector:
    app: postgres
  ports:
    - name: postgres
      port: 5432
      targetPort: 5432
Aplique:

bash
kubectl apply -f postgres.yaml
Verifique o Deployment e o Pod:

bash
kubectl rollout status deployment/postgres --timeout=120s
kubectl get pods -l app=postgres
kubectl get service postgres
kubectl get endpoints postgres
O que queremos ver:

Deployment com uma réplica disponível.

Pod em estado Running e READY 1/1.

Service postgres com a porta 5432.

Endpoint associado ao IP do Pod.

Se o Pod não ficar pronto, examine:

bash
kubectl describe pod -l app=postgres
kubectl logs deployment/postgres
7.3. Por que usar um Service?
O Pod pode ser recriado e receber outro IP. O Service fornece um nome estável, postgres, que a aplicação usará para se conectar.

Dentro do namespace minha-app, a conexão será:

text
postgres:5432
Isso não expõe o banco à sua máquina nem à Internet. ClusterIP é o tipo padrão de Service para comunicação interna do cluster.

7.4. Entendendo as probes do PostgreSQL
pg_isready verifica se o servidor PostgreSQL está aceitando conexões. Isso é útil, mas não prova que:

As credenciais usadas pela API estão corretas.

O schema da aplicação existe.

As migrações foram executadas.

Uma consulta de negócio funciona.

Por isso, a probe da aplicação terá outra finalidade: verificar se a própria API está apta a receber requisições.

Também vale observar que uma falha de liveness repetida reinicia o container. Para um banco, precisamos escolher verificações que detectem falhas reais sem provocar reinicializações desnecessárias.

8. Etapa 6 — implantar o Elasticsearch
O Elasticsearch requer mais atenção do que um container HTTP simples, principalmente por causa de memória, limites do sistema operacional e configuração de segurança.

Para este laboratório, vou assumir uma instalação de nó único. Use a mesma versão principal que você já executa no Compose.

8.1. Criar o Deployment
Crie elasticsearch.yaml:

yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: elasticsearch
  namespace: minha-app
spec:
  replicas: 1
  selector:
    matchLabels:
      app: elasticsearch
  template:
    metadata:
      labels:
        app: elasticsearch
    spec:
      containers:
        - name: elasticsearch
          image: docker.elastic.co/elasticsearch/elasticsearch:8.17.0
          ports:
            - name: http
              containerPort: 9200
          env:
            - name: discovery.type
              value: single-node
            - name: ES_JAVA_OPTS
              value: "-Xms512m -Xmx512m"
            - name: xpack.security.enabled
              value: "false"
          resources:
            requests:
              cpu: "500m"
              memory: "1Gi"
            limits:
              memory: "2Gi"
A imagem acima é apenas uma referência concreta para o exemplo. Ajuste-a para a versão do Elasticsearch que você já utiliza. A configuração desabilita a segurança HTTP e é apropriada apenas para um laboratório isolado, sem dados sensíveis. Não a replique em produção.

Os limites de memória também são exemplos; confira os requisitos da versão e a memória disponível no Minikube.

8.2. Criar o Service
No mesmo arquivo, após ---:

yaml
apiVersion: v1
kind: Service
metadata:
  name: elasticsearch
  namespace: minha-app
spec:
  type: ClusterIP
  selector:
    app: elasticsearch
  ports:
    - name: http
      port: 9200
      targetPort: 9200
Aplique e valide:

bash
kubectl apply -f elasticsearch.yaml
kubectl get pods -l app=elasticsearch -w
Quando o Pod estiver Running, confira:

bash
kubectl get service elasticsearch
kubectl get endpoints elasticsearch
kubectl logs deployment/elasticsearch
Teste a API HTTP do Elasticsearch com um encaminhamento temporário de porta:

bash
kubectl port-forward service/elasticsearch 9200:9200
Em outro terminal:

bash
curl http://localhost:9200/
Se o Elasticsearch estiver configurado com autenticação ou HTTPS, adapte o teste. Não desabilite a segurança apenas para fazer o curl funcionar.

Ponto de aprendizagem: o Elasticsearch pode demorar a inicializar e pode ser encerrado pelo sistema se ultrapassar os limites de memória. Se isso acontecer, veja kubectl describe pod e procure eventos como OOMKilled. Não aumente os recursos às cegas: compare o consumo com a memória que o Minikube realmente possui.

9. Etapa 7 — implantar a API com 3 réplicas
Agora chegamos ao componente central do laboratório.

Antes de criar o Deployment, precisamos conhecer três detalhes da sua aplicação:

A referência da imagem, por exemplo minha-api:1.0.0.

A porta em que a aplicação escuta dentro do container.

Os endpoints que podemos usar para readiness e liveness.

Vou usar minha-api:1.0.0, porta 8080, /health/ready e /health/live como exemplos. Substitua esses valores pelos da sua aplicação antes de aplicar o manifesto. Se você não tem endpoints de saúde, não invente esses caminhos: vamos adaptar as probes à tecnologia utilizada.

9.1. Disponibilizar a imagem local
Se a imagem já existe no Podman, liste-a:

bash
podman images
Se necessário, construa a imagem da API a partir do seu projeto:

bash
podman build -t minha-api:1.0.0 .
Carregue-a no Minikube:

bash
minikube image load minha-api:1.0.0
Valide a imagem:

bash
minikube image ls
O objetivo é garantir que o cluster consiga iniciar containers com essa imagem sem depender de um registry remoto.

No Deployment, usaremos imagePullPolicy: IfNotPresent. Isso permite usar a imagem local quando ela já estiver disponível. Se você atualizar a imagem mantendo exatamente a mesma tag, precisará garantir que a versão correta foi carregada; para o exercício de rolling update, usaremos tags distintas.

9.2. Criar o Deployment e o Service da API
Crie api.yaml:

yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: minha-api
  namespace: minha-app
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: minha-api
  template:
    metadata:
      labels:
        app: minha-api
    spec:
      containers:
        - name: api
          image: minha-api:1.0.0
          imagePullPolicy: IfNotPresent
          ports:
            - name: http
              containerPort: 8080
          envFrom:
            - configMapRef:
                name: minha-app-config
            - secretRef:
                name: minha-app-secrets
          readinessProbe:
            httpGet:
              path: /health/ready
              port: http
            initialDelaySeconds: 10
            periodSeconds: 5
            timeoutSeconds: 2
            failureThreshold: 3
          livenessProbe:
            httpGet:
              path: /health/live
              port: http
            initialDelaySeconds: 20
            periodSeconds: 10
            timeoutSeconds: 2
            failureThreshold: 3
          resources:
            requests:
              cpu: "100m"
              memory: "128Mi"
            limits:
              memory: "512Mi"
---
apiVersion: v1
kind: Service
metadata:
  name: minha-api
  namespace: minha-app
spec:
  type: ClusterIP
  selector:
    app: minha-api
  ports:
    - name: http
      port: 8080
      targetPort: http
Os limites de recursos são ilustrativos. Ajuste-os de acordo com o consumo real da sua aplicação.

Aplique:

bash
kubectl apply -f api.yaml
Espere pelo rollout:

bash
kubectl rollout status deployment/minha-api --timeout=180s
E valide:

bash
kubectl get deployment minha-api
kubectl get replicasets
kubectl get pods -l app=minha-api -o wide
kubectl get service minha-api
kubectl get endpoints minha-api
O resultado esperado é um Deployment com 3/3 réplicas disponíveis e três Pods prontos. O Service deverá ter os três Pods como endpoints elegíveis.

9.3. Testar a API sem o Ingress
Antes de configurar o acesso externo, teste a comunicação com o Service:

bash
kubectl port-forward service/minha-api 8080:8080
Em outro terminal, use um endpoint real da aplicação:

bash
curl -i http://localhost:8080/
Se a aplicação não responder em /, teste uma rota que realmente exista.

Se falhar, examine:

bash
kubectl logs deployment/minha-api
kubectl describe deployment minha-api
kubectl get pods -l app=minha-api
9.4. Aprender a diferença entre readiness e liveness
Readiness probe

Decide se o Pod pode receber tráfego. Se falhar, o Kubernetes retira o Pod dos endpoints elegíveis do Service, mas não necessariamente o reinicia.

Liveness probe

Detecta se o container parece estar travado ou incapaz de se recuperar. Se falhar repetidamente, o kubelet reinicia o container.

Uma boa prática é não fazer a liveness depender diretamente da disponibilidade momentânea do PostgreSQL ou do Elasticsearch. Se um serviço externo ficar indisponível, reiniciar todas as réplicas da API em sequência pode piorar a situação.

A readiness pode verificar dependências essenciais se esse for o comportamento desejado para sua aplicação. A liveness, por sua vez, deve verificar a saúde interna do processo.

Se a API demora muito para iniciar, considere adicionar uma startupProbe. Ela evita que a liveness encerre o container antes de ele concluir a inicialização.

9.5. Verificar a comunicação entre os serviços
Confira se os três Services existem:

bash
kubectl get services
Dentro da rede do cluster, os endereços serão:

text
minha-api:8080
postgres:5432
elasticsearch:9200
O código da aplicação deve usar esses endereços internos, e não localhost, para conectar aos outros Pods.

localhost dentro do container da API significa o próprio container, não o banco ou o Elasticsearch.

10. Etapa 8 — habilitar o Ingress NGINX
O Ingress é o recurso que declara regras HTTP. O controlador NGINX é quem efetivamente implementa essas regras.

Ative o addon do Minikube:

bash
minikube addons enable ingress
Valide a instalação:

bash
kubectl get pods -n ingress-nginx
kubectl get services -n ingress-nginx
Espere até que o controlador esteja pronto:

bash
kubectl wait \
  --namespace ingress-nginx \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/component=controller \
  --timeout=120s
O Minikube disponibiliza esse controlador por meio do addon ingress. 

Ingress-Nginx Controller
+1

10.1. Criar a regra para /api
Crie ingress.yaml:

yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: minha-api
  namespace: minha-app
spec:
  ingressClassName: nginx
  rules:
    - http:
        paths:
          - path: /api
            pathType: Prefix
            backend:
              service:
                name: minha-api
                port:
                  number: 8080
Aplique e valide:

bash
kubectl apply -f ingress.yaml
kubectl get ingress
kubectl describe ingress minha-api
Essa regra encaminha requisições para o Service minha-api que começam com /api. Ela não remove automaticamente o prefixo /api.

Por exemplo, uma requisição a /api/clientes chegará à aplicação com esse caminho. Se a API espera /clientes, precisaremos configurar um rewrite no NGINX ou ajustar as rotas da aplicação. Vamos testar o comportamento real antes de adicionar essa complexidade.

10.2. Testar o Ingress localmente
Uma maneira simples e previsível de testar é usar o encaminhamento de porta do controlador:

bash
kubectl port-forward \
  --namespace ingress-nginx \
  service/ingress-nginx-controller \
  8080:80
Em outro terminal:

bash
curl -i http://localhost:8080/api
Use também uma rota real, como /api/health, se existir.

Se você receber 404, isso não prova sozinho que o Ingress falhou: pode ser a própria aplicação que não conhece a rota solicitada. Confira o controlador, o Service e os logs para identificar em qual camada a resposta foi produzida.

Para testar por um endereço IP ou hostname do cluster, a configuração de rede pode variar conforme o sistema operacional e o driver do Minikube. O encaminhamento de porta evita misturar esse problema com o aprendizado das regras HTTP.

11. Etapa 9 — executar um rolling update
Agora vamos exercitar uma das funcionalidades mais úteis do Deployment.

A estratégia configurada no manifesto foi:

yaml
strategy:
  type: RollingUpdate
  rollingUpdate:
    maxSurge: 1
    maxUnavailable: 0
Isso permite criar até um Pod adicional durante a atualização e não autoriza que o Deployment reduza voluntariamente o número de réplicas disponíveis abaixo do desejado durante o rollout. O sucesso também depende de recursos disponíveis, probes corretas e da aplicação conseguir iniciar.

11.1. Preparar a segunda versão
Crie uma versão nova da imagem com uma tag diferente. Por exemplo:

bash
podman build -t minha-api:1.1.0 .
minikube image load minha-api:1.1.0
Esse exemplo pressupõe que o código já foi alterado para representar a versão 1.1.0. Para observar diferenças entre as versões, pode ser útil expor uma informação de versão em um endpoint ou log.

11.2. Atualizar a imagem
Execute:

bash
kubectl set image deployment/minha-api \
  api=minha-api:1.1.0
Observe o processo:

bash
kubectl rollout status deployment/minha-api
kubectl rollout history deployment/minha-api
kubectl get pods -l app=minha-api -w
O esperado é que o ReplicaSet antigo seja substituído progressivamente pelo novo. As três réplicas devem voltar a ficar prontas na nova versão.

11.3. Fazer rollback
Se a versão nova apresentar um