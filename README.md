# OrderingSystem

This is a recruitment task. 

The project contains two microservices:

**OrderProducer** - creates order creation messages and sends them to the RabbitMQ queue. 

**OrderConsumer** - receives orders from RabbitMQ, calculates the value of the order and writes it to the PostgreSQL database.

## How to run
Download Docker from https://docs.docker.com/get-started/get-docker/ and install.

Go to project folder. In command line type:

`docker-compose up`

The .containers/logs folder contains the logs.

## Tech Stack
.Net 6, RabbitMQ, PostgreSQL, Docker, Entity Framework, Serilog, XUnit, Moq, Fluent Assertions
