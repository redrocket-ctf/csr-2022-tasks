import string
import random
import multiprocessing
from rsa import miller_rabin

import string
def randomString(stringLength=10):
    """Generate a random string of fixed length """
    letters = string.ascii_lowercase + string.ascii_uppercase + string.digits
    return ''.join(random.choice(letters) for i in range(stringLength))

def search():
    random.seed(open('/dev/urandom', 'rb').read(20))
    while True:
        name = f'willi_{randomString(7)}'
        value = int.from_bytes(bytearray(name, 'utf-8'), byteorder='big')
        if miller_rabin(value, 7):
            with (open('names', 'a+')) as f:
                f.write(name + '\n')

for _ in range(5):
    multiprocessing.Process(target=search).start()

search()