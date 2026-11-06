minikube start --driver=podman --container-runtime=containerd --cpus=3 --disk-size=30g
kubectl config set-context --current --namespace=products