import gevent.monkey
gevent.monkey.patch_all()
from signalrcore.hub_connection_builder import HubConnectionBuilder
import logging
import sys

handler = logging.StreamHandler()
handler.setLevel(logging.DEBUG)


hub_connection = HubConnectionBuilder()\
    .with_url("http://localhost:5000/riskhub", options={"verify_ssl": False}) \
    .configure_logging(logging.DEBUG, socket_trace=True, handler=handler) \
    .with_automatic_reconnect({
            "type": "interval",
            "keep_alive_interval": 10,
            "intervals": [1, 3, 5, 6, 7, 87, 3]
        }).build()

hub_connection.on_open(lambda: print("we are connected"))
hub_connection.on_close(lambda: print("we are not connected"))

hub_connection.start()

def signup():
    hub_connection.send("Signup", ["Andy"])
signup()




hub_connection.stop()


