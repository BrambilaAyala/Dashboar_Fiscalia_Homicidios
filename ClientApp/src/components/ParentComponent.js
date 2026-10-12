import React from "react";
import Homicidios from "./Homicidios";

const ParentComponent = () => {
    const datos = [
        { Entidad: "Entidad1", Municipio: "Municipio1", Hombres: 10 },
        { Entidad: "Entidad2", Municipio: "Municipio2", Hombres: 20 },
        // Agrega más datos según sea necesario
    ];

    console.log("Datos en el componente padre:", datos);

    return <Homicidios datos={datos} />;
};

export default ParentComponent;