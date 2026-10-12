-- =============================================================================
-- PROYECTO: Dashboard_Fiscalia_Homicidios (ProyectoFisca)
-- SCRIPT: 03_datos_muestra_homicidios.sql
-- DESCRIPCIÓN: Datos iniciales de homicidios dolosos para pruebas del Dashboard
-- =============================================================================

USE fiscalia;

-- Insertar datos de prueba vinculados a los municipios existentes
INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-01', 3, 2, 1, 0, 'El Mexicano'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'BAJA CALIFORNIA' AND m.nombre = 'TIJUANA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-01', 2, 2, 0, 0, 'La Jornada'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'BAJA CALIFORNIA' AND m.nombre = 'MEXICALI' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-02', 4, 3, 1, 0, 'El Norte'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'NUEVO LEÓN' AND m.nombre = 'MONTERREY' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-02', 1, 1, 0, 0, 'El Porvenir'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'NUEVO LEÓN' AND m.nombre = 'GUADALUPE' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-03', 5, 4, 1, 0, 'Periódico Correo'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'GUANAJUATO' AND m.nombre = 'LEÓN' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-03', 2, 2, 0, 0, 'El Sol de Irapuato'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'GUANAJUATO' AND m.nombre = 'IRAPUATO' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-04', 3, 3, 0, 0, 'Mural'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'JALISCO' AND m.nombre = 'GUADALAJARA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-04', 2, 1, 1, 0, 'El Occidental'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'JALISCO' AND m.nombre = 'ZAPOPAN' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-05', 3, 2, 1, 0, 'El Imparcial'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'SONORA' AND m.nombre = 'HERMOSILLO' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-05', 4, 4, 0, 0, 'Tribuna del Yaqui'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'SONORA' AND m.nombre = 'CAJEME' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-06', 2, 1, 1, 0, 'El Universal'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'CIUDAD DE MÉXICO' AND m.nombre = 'IZTAPALAPA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-06', 1, 1, 0, 0, 'Reforma'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'CIUDAD DE MÉXICO' AND m.nombre = 'GUSTAVO A. MADERO' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-07', 4, 3, 0, 1, 'El Sol de Toluca'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'MÉXICO' AND m.nombre = 'ECATEPEC DE MORELOS' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-07', 2, 2, 0, 0, 'Milenio Edomex'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'MÉXICO' AND m.nombre = 'NEZAHUALCÓYOTL' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-08', 3, 2, 1, 0, 'El Siglo de Torreón'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'COAHUILA DE ZARAGOZA' AND m.nombre = 'TORREÓN' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-08', 3, 3, 0, 0, 'La Voz de Michoacán'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'MICHOACÁN DE OCAMPO' AND m.nombre = 'MORELIA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-03-09', 2, 2, 0, 0, 'Diario de Xalapa'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'VERACRUZ DE IGNACIO DE LA LLAVE' AND m.nombre = 'XALAPA' LIMIT 1;

-- Datos representativos para el análisis trimestral (2023 y 2024)
INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2023-02-15', 6, 5, 1, 0, 'Histórico SSPC Q1 2023'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'GUANAJUATO' AND m.nombre = 'LEÓN' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2023-05-20', 8, 7, 1, 0, 'Histórico SSPC Q2 2023'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'BAJA CALIFORNIA' AND m.nombre = 'TIJUANA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2023-08-11', 5, 4, 1, 0, 'Histórico SSPC Q3 2023'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'JALISCO' AND m.nombre = 'GUADALAJARA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2023-11-28', 7, 6, 1, 0, 'Histórico SSPC Q4 2023'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'MÉXICO' AND m.nombre = 'ECATEPEC DE MORELOS' LIMIT 1;
