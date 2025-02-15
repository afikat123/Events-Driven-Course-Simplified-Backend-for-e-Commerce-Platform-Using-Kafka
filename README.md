This project consists of a producer and a consumer service that communicate via Kafka. The producer: 
  1. Generates orders and publishes them to a Kafka topic named **new-orders-topic**
  2. Update existing orders and publish them to a Kafka topic names **update-orders-topic**
 while the consumer processes these orders.

## Author

**Student 1**

**Full Name:** Afik Atias

**ID Number:** 318515954

**Student 2**

**Full Name:** Roni Kuznicki

**ID Number:** 317500544

## Running Instructions

1. **Create the internal network and Run the docker-compose.yml:**

    ```sh
    docker-compose -f docker-compose.yml up --build
    ```

## API Endpoints

### Producer

- **Create Order**

    **URL:** `localhost:5000/cart/create-order`

    **Method:** `POST`

    **Query Params** :`orderId=string, itemsNum=integer`

    **Example:** `localhost:5000/cart/create-order?orderId=afik&itemsNum=3`

    **Response:**

    `Order published!`

- **Update Order**

    **URL:** `localhost:5000/cart/update-order`

    **Method:** `POST`

    **Query Params** :`orderId=string, newStatus=string`

    **Example:** `localhost:5000/cart/update-order?orderId=afik&newStatus=done`

    **Response:**

    `Order Updated!`

### Consumer

- **Get Order Details**

    **URL:** `localhost:5001/order/order-details`

    **Method:** `GET`

    **Query Params:** `orderId=string`

    **Example:** `localhost:5001/order/order-details?orderId=afik`

    **Response:**

    ```json
    {
    "orderId": "string",
    "totalAmount": "decimal",
    "shippingCost": "decimal",
    "items": [
        "Item1",
        "Item2",
        "Item3"
        "...."
    ]
}
    ```

- **Get All OrderId From Topic**

    **URL:** `localhost:5001/order/getAllOrderIdsFromTopic`

    **Method:** `GET`

    **Query Params:** `topic=string`

    **Example:** `localhost:5001/order/getAllOrderIdsFromTopic?topic=update-orders-topic`

    **Response:**

    ```json
    [
        "Item1",
        "Item2",
        "Item3"
        "...."
    ]
    ```

## Kafka Configuration
  ## Topics:
    1. new-orders-topic
    2. update-order-topic

  ## Message Key:
  -  for every message in produced we used the orderId as the key to the message, to ensure that every consumer will handle every message related to the same order(create and update)
  ## Handle Errors:
  **Producer:**
  -  Input Validtion: Endpoints check that the requiered params are in the correct type and return the correct error code
  -  Connection Validation: The producer app is starting only after the Kafka app is up, and then produce a healthcheck message to verify the connetcion is valid
  **Consumer:**
  -  Connection Validtion: the consumer app is starting after the producer app is up
  -  Event Processing: every event is procced with a try-catch block to catach potential errors and handle them
  -  Input Validtion: Endpoints check that the requiered params are in the correct type and return the correct error code
    
  
