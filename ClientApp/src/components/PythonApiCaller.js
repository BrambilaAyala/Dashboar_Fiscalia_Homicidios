// ClientApp/src/Components/PythonApiCaller.js
import React, { useState } from 'react';
import axios from 'axios';

const PythonApiCaller = () => {
    const [response, setResponse] = useState(null);

    const callPythonApi = async () => {
        try {
            const res = await axios.post('http://localhost:5000/run-script', {
                mensaje: 'Solicitud desde React'
            });
            setResponse(res.data);
        } catch (error) {
            console.error('Error al llamar a la API de Python:', error);
        }
    };

    return (
        <div>
            <button onClick={callPythonApi}>Llamar a la API de Python</button>
            {response && <pre>{JSON.stringify(response, null, 2)}</pre>}
        </div>
    );
};

export default PythonApiCaller;

