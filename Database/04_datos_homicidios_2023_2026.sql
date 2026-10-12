-- =============================================================================
-- PROYECTO: Dashboard_Fiscalia_Homicidios (ProyectoFisca)
-- SCRIPT: 04_datos_homicidios_2023_2026.sql
-- DESCRIPCIÓN: Carga de registros oficiales y forenses para el periodo 2023 - 2026
-- =============================================================================

USE fiscalia;

-- =============================================================================
-- AÑO 2026 (Boletines Recientes SSPC / CNS)
-- =============================================================================
INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2026-08-30', 3, 2, 1, 0, 'El Imparcial Sonora'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'SONORA' AND m.nombre = 'HERMOSILLO' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2026-08-30', 4, 3, 1, 0, 'Tribuna del Yaqui'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'SONORA' AND m.nombre = 'CAJEME' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2026-08-29', 5, 4, 1, 0, 'Periódico Correo'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'GUANAJUATO' AND m.nombre = 'CELAYA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2026-08-28', 6, 5, 1, 0, 'El Mexicano'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'BAJA CALIFORNIA' AND m.nombre = 'TIJUANA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2026-07-15', 3, 2, 0, 1, 'Mural Jalisco'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'JALISCO' AND m.nombre = 'TLAQUEPAQUE' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2026-06-20', 4, 4, 0, 0, 'El Norte Monterrey'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'NUEVO LEÓN' AND m.nombre = 'JUÁREZ' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2026-05-12', 3, 2, 1, 0, 'El Sol de Toluca'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'MÉXICO' AND m.nombre = 'CHIMALHUACÁN' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2026-04-18', 2, 2, 0, 0, 'El Heraldo de Juárez'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'CHIHUAHUA' AND m.nombre = 'JUÁREZ' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2026-03-10', 4, 3, 1, 0, 'Noroeste Culiacán'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'SINALOA' AND m.nombre = 'CULIACÁN' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2026-02-14', 3, 2, 1, 0, 'El Sur de Acapulco'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'GUERRERO' AND m.nombre = 'ACAPULCO DE JUÁREZ' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2026-01-22', 2, 1, 1, 0, 'El Universal CDMX'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'CIUDAD DE MÉXICO' AND m.nombre = 'CUAUHTÉMOC' LIMIT 1;

-- =============================================================================
-- AÑO 2025 (Boletines Oficiales SSPC / CNS)
-- =============================================================================
INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2025-11-14', 5, 4, 1, 0, 'Periódico Correo'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'GUANAJUATO' AND m.nombre = 'SALAMANCA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2025-10-05', 4, 3, 0, 1, 'La Voz de Michoacán'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'MICHOACÁN DE OCAMPO' AND m.nombre = 'URUAPAN' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2025-09-18', 3, 2, 1, 0, 'El Sol de Zacatecas'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'ZACATECAS' AND m.nombre = 'FRESNILLO' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2025-08-22', 6, 5, 1, 0, 'Frontera Tijuana'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'BAJA CALIFORNIA' AND m.nombre = 'TIJUANA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2025-07-30', 4, 3, 1, 0, 'El Debate Mazatlán'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'SINALOA' AND m.nombre = 'MAZATLÁN' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2025-06-11', 2, 2, 0, 0, 'Novedades de Quintana Roo'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'QUINTANA ROO' AND m.nombre = 'BENITO JUÁREZ' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2025-05-04', 3, 3, 0, 0, 'El Sol de Puebla'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'PUEBLA' AND m.nombre = 'PUEBLA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2025-04-19', 4, 3, 1, 0, 'El Siglo de Torreón'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'COAHUILA DE ZARAGOZA' AND m.nombre = 'SALTILLO' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2025-03-25', 5, 4, 1, 0, 'El Imparcial'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'SONORA' AND m.nombre = 'NOGALES' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2025-02-17', 2, 1, 1, 0, 'El Diario de Juárez'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'CHIHUAHUA' AND m.nombre = 'CHIHUAHUA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2025-01-08', 3, 2, 1, 0, 'El Mañana de Reynosa'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'TAMAULIPAS' AND m.nombre = 'REYNOSA' LIMIT 1;

-- =============================================================================
-- AÑO 2024 (Boletines Oficiales SSPC / CNS)
-- =============================================================================
INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-11-20', 4, 3, 1, 0, 'El Dictamen Veracruz'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'VERACRUZ DE IGNACIO DE LA LLAVE' AND m.nombre = 'VERACRUZ' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-10-15', 3, 3, 0, 0, 'El Heraldo de Chiapas'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'CHIAPAS' AND m.nombre = 'TUXTLA GUTIÉRREZ' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-09-02', 5, 4, 1, 0, 'El Sol de Morelia'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'MICHOACÁN DE OCAMPO' AND m.nombre = 'ZAMORA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-06-14', 4, 3, 1, 0, 'El Occidental'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'JALISCO' AND m.nombre = 'TONALÁ' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2024-05-18', 3, 2, 0, 1, 'Milenio Estado de México'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'MÉXICO' AND m.nombre = 'NAUCALPAN DE JUÁREZ' LIMIT 1;

-- =============================================================================
-- AÑO 2023 (Boletines Oficiales SSPC / CNS)
-- =============================================================================
INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2023-12-05', 4, 3, 1, 0, 'Periódico Correo'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'GUANAJUATO' AND m.nombre = 'IRAPUATO' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2023-10-12', 3, 2, 1, 0, 'El Imparcial'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'SONORA' AND m.nombre = 'SAN LUIS RÍO COLORADO' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2023-09-24', 5, 4, 1, 0, 'La Jornada Baja California'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'BAJA CALIFORNIA' AND m.nombre = 'ENSENADA' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2023-07-07', 4, 3, 0, 1, 'Noroeste'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'SINALOA' AND m.nombre = 'AHOME' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2023-04-16', 3, 2, 1, 0, 'El Sol de Tampico'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'TAMAULIPAS' AND m.nombre = 'MATAMOROS' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2023-03-21', 2, 2, 0, 0, 'El Financiero'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'CIUDAD DE MÉXICO' AND m.nombre = 'BENITO JUÁREZ' LIMIT 1;

INSERT INTO Homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
SELECT m.id, '2023-01-10', 4, 3, 1, 0, 'El Norte'
FROM Municipio m JOIN Entidad e ON m.entidad_id = e.id
WHERE e.nombre = 'NUEVO LEÓN' AND m.nombre = 'APODACA' LIMIT 1;
