# Technical Architecture

A high level view of the technical architecture of the Teaching Record System. Note: This view does not include "legacy" technical debt soon to be removed.
```mermaid
architecture-beta
service usr(internet)[User]
group tscloud(cloud)[Teacher Services Cloud]
service tscfrontdoor(server)[TS Front Door CDN] in tscloud
service tscloadbalancers(server)[TS Load Balancers] in tscloud
group clustervn(cloud)[Cluster Virtual Network] in tscloud
service nginx(server)[TS Nginx ingress controller] in clustervn
group aks(server)[TRS AKS Pod Workloads] in clustervn
service trsapi(server)[TRS API] in aks
service trsworker(server)[TRS Worker] in aks
service trsui(server)[TRS Support UI] in aks
service trsauth(server)[TRS Teaching Auth] in aks
group trs(cloud)[TRS Azure Resource Group] in clustervn
service db(database)[PostgreSQL] in trs
service trsstorage(disk)[Storage Account] in trs
service trsredis(server)[Redis Cache] in trs
service trskeyvaults(server)[TRS Key Vaults] in trs
tscfrontdoor:R --> L:tscloadbalancers
tscloadbalancers{group}:B --> T:nginix{group}
usr:B --> T: tscfrontdoor{group}
```
