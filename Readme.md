# 🛒 Mini Marketplace (Test Task)

This project is a mini marketplace built with a microservices + onion architecture and various technologies.  

## 🧩 Services

- **Zookeeper** and **Kafka** → message broker for communication between services  
- **MongoDB** → main database for products and orders  
- **Redis** → cache for search results and temporary data  
- **Elasticsearch** → indexing and searching products and orders  
- **Product Service** → manages products  
- **Order Service** → manages orders  
- **Auth Service** → handles user authentication  
- **Search Service** → search functionality using ES and Redis  
- **API Gateway** → single entry point for all services  

API Gateway → http://localhost:3000

Product Service → http://localhost:8002

Order Service → http://localhost:8003

Auth Service → http://localhost:8000

Search Service → http://localhost:8004

Elasticsearch → http://localhost:9200

Redis → localhost:6379

MongoDB → localhost:27017

Kafka → localhost:29092 (for clients)

## 🚀 Run

To start the project:

```bash
docker-compose up -d --build
