import React, { useEffect, useState } from 'react';
import axios from 'axios';

const TableView = () => {
    const [data, setData] = useState([]);

    useEffect(() => {
        axios.get('/api/data')
            .then(response => {
                setData(response.data);
            })
            .catch(error => {
                console.error('Error fetching data:', error);
            });
    }, []);

    return (
        <div>
            <h1>Datos de Homicidios</h1>
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Municipio</th>
                        <th>Fecha</th>
                        <th>Número de Muertos</th>
                        <th>Hombres</th>
                        <th>Mujeres</th>
                        <th>No Identificado</th>
                        <th>Fuente</th>
                    </tr>
                </thead>
                <tbody>
                    {data.map((row, index) => (
                        <tr key={index}>
                            <td>{row.id}</td>
                            <td>{row.municipio_id}</td>
                            <td>{row.fecha}</td>
                            <td>{row.num_muertos}</td>
                            <td>{row.hombres}</td>
                            <td>{row.mujeres}</td>
                            <td>{row.no_identificado}</td>
                            <td>{row.fuente}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default TableView;