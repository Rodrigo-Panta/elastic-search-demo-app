# Run Kubernetes Cluster Locally with Minikube and Podman

## Overview

This project sets and configures a K8s Cluster with minikube with the following architecture:

```mermaid
@TODO
```

## Index

- [1. Start the cluster](#1-start-the-cluster)
- [2. Create a namespace](#2-create-a-namespace)
- [3. Create a ConfigMap and a Secret file](#3-create-a-configmap-and-a-secret-file)
- [4. Create a deployment for the Postgresql Database](#4-create-a-deployment-for-the-postgresql-database)
- [5. Create a deployment for Elastic Search](#5-create-a-deployment-for-elastic-search)
- [6. Create the API with 3 replicas](#6-create-the-api-with-3-replicas)
- [7. Enable the NGINX Ingress](#7-enable-the-nginx-ingress)
- [Troubleshooting](#troubleshooting)

## 1. Start the cluster

After installing minikube and kubectl, run:

```bash
minikube config set rootless true                                  
minikube start --driver=podman --container-runtime=containerd --cpus=3 --disk-size=30g
```

Validate the cluster is up and running:

```bash
minikube status
kubectl config current-context
kubectl get nodes -o wide
kubectl get pods -A
```

## 2. Create a namespace

Create a new file `k8s/namespace.yaml` and apply with kubectl:

```bash
kubectl create -f ./k8s/namespace.yaml
```

or

```bash
kubectl apply -f ./k8s/namespace.yaml
```

Validate:

```bash
kubectl get namespaces
kubectl describe namespace products
```

Define it as default:

```bash
kubectl config set-context --current --namespace=products
kubectl config view --minify | grep namespace #verify
```

## 3. Create a ConfigMap and a Secret file

- ConfigMap:

 ```bash
 kubectl apply -f configmap.yaml
 kubectl get configmap
 kubectl describe configmap products-config
 ```

- Secret:

 ```bash
 kubectl apply -f secret.yaml
 kubectl get secrets
  ```

- To check the keys without printing the values:

 ```bash
 kubectl describe secret products-secrets
 ```

> NOTE: The secrets object is not encrypted by default. It is simply stored as a base 64 string. Check the documentation for encryption or other secret handling methods, such as key vaults.

- Check both:

```bash
kubectl get configmap,secret
```

## 4. Create a deployment for the Postgresql Database

> NOTE: for this lab, the database will be created as a Deployment without persistence configuration. A further exercise to this project could be to implement a Statefulset with a PersistentVolume for the database.

Create a `postgres.yaml` file.

Define the `deployment` and the `service` for the postgreSQL.

Use the secrets and environment variables described in the ConfigMap and Secret files.

Apply:

```bash
kubectl apply -f postgres.yaml
```

Verify the deployment and POD:

```bash
kubectl rollout status deployment/postgres --timeout=120s
kubectl get pods -l app=postgres
kubectl get service postgres
kubectl get endpoints postgres
```

If the Pod doesn't get to the status "ready", troubleshoot:

```bash
kubectl describe pod -l app=postgres
kubectl logs deployment/postgres
```

## 5. Create a deployment for Elastic Search

Create the `elasticsearch.yaml` with the `deployment` and `service` specs;

## 6. Create the API with 3 replicas

First, build the image and either push it to the registry or load it directly to minikube

### Build the image


```bash
podman build -t rodrigopanta/products-api:v1
```

### 1. Push it to the registry

```bash
podman tag rodrigopanta/products-api:v1 docker.io/rodrigopanta/products-api:latest
podman tag rodrigopanta/products-api:v1 docker.io/rodrigopanta/products-api:v1
podman login docker.io
podman push docker.io/rodrigopanta/products-api:latest
podman push docker.io/rodrigopanta/products-api:v1
```

### 2. Or load it into minikube

```bash
minikube image load rodrigopanta/products-api:v1
```

Verify:

```bash
minikube image ls
```

### Create a deployment for the API

Create the `api.yaml` file with the `Deployment` and the `Service` for the API.

Apply:

```bash
kubectl apply -f api.yaml
```

Wait for the rollout:

```bash
kubectl rollout status deployment/products-api
```

You can do a port forward to test the API now.

```bash
kubectl port-forward service/products-api 8080:8080
```

## 7. Enable the NGINX Ingress

### Enable the `ingress minikube addon`

```bash
minikube addons enable ingress
```

Validate the instalation:

```bash
kubectl get pods -n ingress-nginx
kubectl get services -n ingress-nginx
```

### Create the Api Rule

Create `ingress.yaml`, apply and describe:

```bash
kubectl apply -f ingress.yaml
kubectl get ingress
kubectl describe ingress products-ingress
```

### Validate the ingress locally

Do a port forwarding to test the ingress:

```bash
kubectl port-forward \
  --namespace ingress-nginx \
  service/ingress-nginx-controller \
  8080:80
```

Test the application locally on other terminal:

```bash
curl -i http://localhost:8080/api
```

## 8. Rolling Update

Let's execute a Rolling Update on the Deployment

The strategy configured in the api manifest was:

```yaml
strategy:
  type: RollingUpdate
  rollingUpdate:
    maxSurge: 1
    maxUnavailable: 0
```

It allows us to create one additional Pod during rollouts and prevents the Deployment from deliberately reducing the number of available replicas from the desired value.

### Prepare the second version

```bash
podman build -t products-api:v2.0 .
minikube image load products-api:v2.0
```

### Update the image

Execute: 

```bash
kubectl set image deployment/products-api \
  api=products-api:v2.0
```

Watch the process:

```bash
kubectl rollout status deployment/products-api
kubectl rollout history deployment/products-api
kubectl get pods -l app=products-api -w
```

## Troubleshooting 

### Minikube

```bash
minikube profile list
minikube status
minikube ssh
```

### Logs

```bash
kubectl describe pod -l app=[APP_NAME]
kubectl logs deployment/[DEPLOYMENT_NAME]
kubectl get replicasets
kubectl get pods -l app=minha-api -o wide
kubectl get service minha-api
kubectl get endpoints minha-api
```

### Get shell in a running container

```bash
kubectl exec --stdin --tty [POD_NAME] -- /bin/bash
```

### Forward Port

```bash
kubectl port-forward service/[SERVICE_NAME] [PORT]:[PORT]
```

### Restart a deployment:

```bash
kubectl rollout restart deployment -n [NAMESPACE_NAME]
kubectl rollout restart deployment/[DEPLOYMENT_NAME]
```

### Delete resource

```bash
kubectl delete -f api.yaml 
```

### File Watcher problem with .NET

In case the App container throws: 

```txt
System.IO.IOException: The configured user limit (128) on the number of inotify instances has been reached
```


[Check this documentation for understanding](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/?view=aspnetcore-10.0);

Check the current iNotify File System Watchers and File Descriptor Instances on the host machine (docker containers inherit these configs from the host machine): 

```bash
cat /proc/sys/fs/inotify/max_user_instances
cat /proc/sys/fs/inotify/max_user_watches
```

Change them temporarily:

```bash
sudo sysctl fs.inotify.max_user_instances=512
sudo sysctl fs.inotify.max_user_watches=524288
```

Change them indefinetely:

```bash
sudo nano /etc/sysctl.conf
# Add the following lines:
#    fs.inotify.max_user_instances = 512
#    fs.inotify.max_user_watches = 524288
```

