import React from 'react';
import {CookiesProvider} from 'react-cookie'
import Fetch from "./Fetch";
import './App.css'

function App() {
    return (
        <CookiesProvider>
           <Fetch/>
        </CookiesProvider>
    );
}

export default App
