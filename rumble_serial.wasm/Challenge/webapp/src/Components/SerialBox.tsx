import * as React from 'react';
import validate_serial, { SerialModule } from "../serial";
import TextField from '@mui/material/TextField';
import { useState, useEffect } from "react";

export default function SerialBox() {
    const [key, setKey] = useState("");
    const [mod, setMod] = useState<SerialModule>();
    const [error, setError] = useState("");
    const [flag, setFlag] = useState("");

    const keyChanged = async (event: any) => {
        setKey(event.target.value);
    }

    const load_serial = async () => {
        if (!mod) setMod(await validate_serial());
    }

    const format_err = (err:string) : string => {
        return err.split("ERR:")[1].trim();
    }

    useEffect(() => {
        if (mod && key) {
            let res = mod.validate(key);
            if (res == 1) { setError(format_err(mod.stdout)); setFlag(""); }
            else if (res == 0) { setError(""); setFlag(mod.stdout); }
        } else if (!mod) {
            setError("Module not loaded yet");
            //console.error("Module not loaded yet")
        } else if (!key) {
            setError("Empty serial");
        }
    }, [key, mod]);

    useEffect(() => {
        load_serial();
    }, []);

    return (
        <div><TextField
            fullWidth
            error={error != ""}
            id="outlined-basic"
            variant="outlined"
            value={key}
            onChange={keyChanged}
            label="Serial"
            helperText={error?error : ""}
        />
        {flag ? <TextField
        style={{marginTop:"10px"}}
            fullWidth
            id="standard-read-only-input"
            variant="outlined"
            label="Flag"
            InputProps={{
              readOnly: true,
            }}
            value={flag}
          /> : null}
          </div>
    )
}