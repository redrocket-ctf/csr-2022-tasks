import Grid from '@mui/material/Grid';
import IconButton from '@mui/material/IconButton';
import * as React from 'react';
import { useState, useEffect } from 'react';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import PauseIcon from '@mui/icons-material/Pause';
import songs from "./songs.json";
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Link from '@mui/material/Link';

export default function Player() {
    const [playing, setPlaying] = useState(false);
    const [song, setSong] = useState("SKiD ROW - Fallout 3 Operation Anchorag intro.mp3");
    const [audio, _] = useState(new Audio());
    audio.volume = 0.5;
    audio.loop = true;
    const play_toggle = () => {
        setPlaying(!playing);
        if (audio){
            if (!playing) audio.play();
            else audio.pause();
        }
    };

    useEffect(()=>{
        if (song){
            audio.pause();
            audio.src = "music/" + song
            audio.load();
            if (playing) audio.play();
        }
    }, [song]);

    return (
        <Grid container spacing={2}>
        <Grid item xs={2}>
            <IconButton aria-label="delete" onClick={play_toggle}>
                {playing ? <PauseIcon/> : <PlayArrowIcon /> }
            </IconButton>
        </Grid>
        <Grid item xs={10}>
            <FormControl sx={{ m: 1, minWidth: 120 }} size="small">
            <Select
            labelId="demo-simple-select-standard-label"
            id="demo-simple-select-standard"
            value={song}
            onChange={(event)=>setSong(event.target.value)}
            autoWidth
            >
            {songs.map((_song, idx) => <MenuItem key={idx} value={_song}>{_song}</MenuItem>)}
            </Select>
            </FormControl>
        </Grid>
        </Grid>
    )
}