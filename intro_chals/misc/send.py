from nullbot import modules

class CTFModule(modules.Base):
    def __init__(self) -> None:
        super().__init__(0x53)

    def do_write(self):
        self.send(*list(b"init__prog__addr__77_1_2_0\x31\x88\x99CSR{i2c_d0_b3_v3ry_31337}_|>|>|>|>"))

module = CTFModule()
module.do_write()