# Kubernetes Manifestleri

Bu klasör, sistemin Kubernetes üzerinde nasıl deploy edileceğini gösterir.

## Uygulama Adımları (Minikube örneği)

```bash
minikube start
kubectl apply -f k8s/
minikube service eco-queryapi --url