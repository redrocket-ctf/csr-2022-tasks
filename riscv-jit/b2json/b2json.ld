OUTPUT_ARCH("riscv")
ENTRY(_start)

MEMORY {
    mem (wxa) : ORIGIN = 0x00000000, LENGTH = 1M
}

SECTIONS {
    .text : {
        *(.text.start .text.start.*)
        *(.text .text.*)
    } >mem

    .rodata : {
        *(.rodata .rodata.*)
    } >mem

    .data : {
        *(.data .data.*)
    } >mem
}
