import React from 'react';
import {CookiesProvider} from 'react-cookie'
import Fetch from "./Fetch";

function App() {
    

    return (
        <CookiesProvider>
           <Fetch/>
        </CookiesProvider>
    );
}

export default App;
