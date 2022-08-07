# Simple iot data dump

HTTP GET @ `/api/iot` to get all available buckets which contain data

HTTP GET @ `/api/iot/<bucketname>` to get data of bucket in csv format

HTTP POST @ `/api/iot/<bucketname>` to send data to the bucket (sent in body as text/plain)

HTTP DELETE @ `/api/iot/<bucketname>` to delete the whole bucket

