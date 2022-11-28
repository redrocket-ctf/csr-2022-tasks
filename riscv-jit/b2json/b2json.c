typedef unsigned long uintptr_t;
typedef unsigned long size_t;
typedef long ssize_t;

typedef unsigned char uint8_t;
typedef unsigned int uint32_t;

typedef enum {
    KIND_NULL,
    KIND_FALSE,
    KIND_TRUE,
    KIND_NUMBER,
    KIND_STRING,
    KIND_ARRAY,
    KIND_OBJECT,
} val_kind_t;

#define write_lit(lit) write(lit, sizeof lit - 1)

static _Noreturn void sys_exit(void) {
    register uintptr_t num_ asm("a7") = 0;
    asm volatile("ecall" ::"r"(num_) : "memory");
    __builtin_unreachable();
}

static ssize_t sys_read(void *buf, size_t len) {
    register uintptr_t ret asm("a0");
    register uintptr_t num_ asm("a7") = 1;
    register uintptr_t len_ asm("a1") = (uintptr_t)len;
    asm volatile("ecall" : "=r"(ret) : "r"(num_), "0"(buf), "r"(len_) : "memory");
    return (ssize_t)ret;
}

static ssize_t sys_write(void *buf, size_t len) {
    register uintptr_t ret asm("a0");
    register uintptr_t num_ asm("a7") = 2;
    register uintptr_t len_ asm("a1") = (uintptr_t)len;
    asm volatile("ecall" : "=r"(ret) : "r"(num_), "0"(buf), "r"(len_) : "memory");
    return (ssize_t)ret;
}

static void read(void *buf, size_t len) {
    if (sys_read(buf, len) != (ssize_t)len) {
        sys_write("Error: Short read\n", 18);
        sys_exit();
    }
}

static void write(void *buf, size_t len) {
    if (sys_write(buf, len) != (ssize_t)len) {
        sys_write("Error: Short write\n", 19);
        sys_exit();
    }
}

static uint8_t read_8(void) {
    uint8_t val = 0;
    read(&val, sizeof val);
    return val;
}

static uint32_t read_32(void) {
    uint32_t val = 0;
    read(&val, sizeof val);
    return val;
}

static void write_char(char c) {
    write(&c, 1);
}

static void write_int(uint32_t val) {
    write_lit("0x");
    for (size_t i = 0; i < sizeof val; i++) {
        unsigned char b = ((unsigned char *)&val)[sizeof val - i - 1];
        write_char("0123456789abcdef"[b >> 4]);
        write_char("0123456789abcdef"[b & 0xf]);
    }
}

static void print_val(void) {
    uint8_t kind = read_8();
    switch (kind) {
        case KIND_NULL:
            write_lit("null");
            break;
        case KIND_FALSE:
            write_lit("false");
            break;
        case KIND_TRUE:
            write_lit("true");
            break;
        case KIND_NUMBER: {
            write_int(read_32());
            break;
        }
        case KIND_STRING: {
            write_lit("\"");
            uint32_t len = read_32();
            for (size_t i = 0; i < len; i++) {
                uint8_t val = read_8();
                if (val == '\\')
                    write_lit("\\\\");
                else if (val == '"')
                    write_lit("\\\"");
                else
                    write(&val, sizeof val);
            }
            write_lit("\"");
            break;
        }
        case KIND_ARRAY: {
            write_lit("[ ");
            uint32_t len = read_32();
            for (size_t i = 0; i < len; i++) {
                print_val();
                if (i < len - 1)
                    write_lit(", ");
            }
            write_lit(" ]");
            break;
        }
        case KIND_OBJECT: {
            write_lit("{ ");
            uint32_t len = read_32();
            for (size_t i = 0; i < len; i++) {
                print_val();
                write_lit(": ");
                print_val();
                if (i < len - 1)
                    write_lit(", ");
            }
            write_lit(" }");
            break;
        }
        default:
            write_lit("Invalid type\n");
            sys_exit();
            break;
    }
}

static _Noreturn void entry(void) {
    write_lit("Binary-JSON to JSON converter, allocation-free and streaming!\n"
              "Enter raw binary-JSON:\n");
    print_val();
    write_lit("\n");
    sys_exit();
}

__attribute__((naked, used, section(".text.start"))) void _start(void) {
    asm("li sp, 0xfff0 \n"
        "j entry \n" ::"X"(entry));
}
