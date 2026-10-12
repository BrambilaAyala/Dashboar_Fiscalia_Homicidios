SELECT e.nombre AS Entidad, COUNT(*) AS Total
FROM fiscalia.Homicidios h
JOIN fiscalia.Municipio m ON h.municipio_id = m.id
JOIN fiscalia.Entidad e ON m.entidad_id = e.id
WHERE e.nombre IN ('MÉXICO', 'CIUDAD DE MÉXICO', 'COAHUILA DE ZARAGOZA', 'MICHOACÁN DE OCAMPO', 'VERACRUZ DE IGNACIO DE LA LLAVE', 'NUEVO LEÓN', 'SAN LUIS POTOSÍ', 'QUERÉTARO', 'YUCATÁN')
GROUP BY e.nombre
ORDER BY Total DESC;
