SELECT e.nombre, count(h.id) AS homicidios_count
FROM fiscalia.Entidad e
LEFT JOIN fiscalia.Municipio m ON m.entidad_id = e.id
LEFT JOIN fiscalia.Homicidios h ON h.municipio_id = m.id
GROUP BY e.id, e.nombre
ORDER BY homicidios_count DESC;
