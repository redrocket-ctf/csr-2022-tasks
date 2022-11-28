/// <reference types="emscripten" />
/** Above will import declarations from @types/emscripten, including Module etc. */

// This will merge to the existing EmscriptenModule interface from @types/emscripten
// If this doesn't work, try globalThis.EmscriptenModule instead.
export interface SerialModule extends EmscriptenModule {
    // Module.cwrap() will be available by doing this.
    // Requires -s "EXTRA_EXPORTED_RUNTIME_METHODS=['cwrap']"
    cwrap: typeof cwrap;
    // Exported from add.cpp
    // Requires "EXPORTED_FUNCTIONS=['_add']"
    validate(a: string): number;
    readonly stdout: string;
}

export default function validate_serial(mod?: any): SerialModule;