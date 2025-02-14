import React, { useEffect, useState } from "react";
import Chart from "chart.js/auto";

const Homicidios = () => {
  const [datos, setDatos] = useState([]);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch("/homicidios", {
          mode: "cors", // Habilita CORS para obtener más detalles
        });
    
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
    
        const data = await response.json();
        setDatos(data);
      } catch (error) {
        console.error("Error al obtener los datos:", error);
        setError("No se pudieron cargar los datos. Verifica el servidor.");
      }
    };    

    fetchData();
  }, []);

  useEffect(() => {
    if (datos.length === 0) return;

    const ctx = document.getElementById("homicidios").getContext("2d");
    const labels = datos.map((h) => `${h.entidad} - ${h.municipio}`);
    const valores = datos.map((h) => h.num_muertos);

    new Chart(ctx, {
      type: "bar",
      data: {
        labels: labels,
        datasets: [
          {
            label: "Número de Homicidios",
            data: valores,
            backgroundColor: "rgba(75, 192, 192, 0.2)",
            borderColor: "rgba(75, 192, 192, 1)",
            borderWidth: 1,
          },
        ],
      },
      options: {
        responsive: true,
        scales: {
          y: {
            beginAtZero: true,
            title: {
              display: true,
              text: "Número de Homicidios",
            },
          },
          x: {
            title: {
              display: true,
              text: "Entidad - Municipio",
            },
          },
        },
      },
    });
  }, [datos]);

  return (
    <div>
      <canvas id="homicidios" width="400" height="200"></canvas>
      {error ? (
        <p style={{ color: "red" }}>{error}</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Entidad</th>
              <th>Municipio</th>
              <th>Homicidios</th>
            </tr>
          </thead>
          <tbody>
            {datos.map((h, index) => (
              <tr key={index}>
                <td>{h.entidad}</td>
                <td>{h.municipio}</td>
                <td>{h.num_muertos}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default Homicidios;
