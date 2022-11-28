import * as React from 'react';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import CssBaseline from '@mui/material/CssBaseline';
import Link from '@mui/material/Link';
import TextField from '@mui/material/TextField';
import { useState, useEffect } from "react";
import SerialBox from './Components/SerialBox';
import Player from './Components/Player';
import { Random } from "react-animated-text";

import "./cracktro.scss";

export default function App() {

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        minHeight: '100vh',
      }}
    >
      <CssBaseline />
      <Container component="main" sx={{ mt: 8, mb: 2 }} maxWidth="sm">
        <Typography variant="h2" component="h1" gutterBottom>
          <Random text="serial.wasm" effect="fadeOut" effectChange={1.0} effectDuration={1.0}></Random>
        </Typography>
        <Typography variant="subtitle1" component="h2" gutterBottom>
          {"It's easy: enter the serial to get the flag. :)"}
        </Typography>
        <SerialBox></SerialBox>
      </Container>
      <Box
        component="footer"
        sx={{
          py: 3,
          px: 2,
          mt: 'auto',
          backgroundColor: (theme) =>
            theme.palette.mode === 'light'
              ? theme.palette.grey[200]
              : theme.palette.grey[800],
        }}
      >
        <Container maxWidth="sm">
          <Player></Player>
        </Container>
      </Box>
    </Box>
  );
}
