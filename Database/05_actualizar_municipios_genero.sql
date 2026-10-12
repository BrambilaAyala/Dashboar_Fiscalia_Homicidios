USE fiscalia;

-- 1. Actualizar registros existentes para distribuirlos en municipios reales de cada entidad
UPDATE Homicidios h
JOIN Municipio m_old ON h.municipio_id = m_old.id
JOIN Entidad e ON m_old.entidad_id = e.id
SET h.municipio_id = (
    SELECT m_new.id FROM Municipio m_new 
    WHERE m_new.entidad_id = e.id AND m_new.nombre != 'ESTATAL'
    ORDER BY 
        CASE 
            -- Baja California
            WHEN e.id = 2 AND (h.id % 3 = 0) THEN (m_new.nombre = 'TIJUANA')
            WHEN e.id = 2 AND (h.id % 3 = 1) THEN (m_new.nombre = 'MEXICALI')
            WHEN e.id = 2 THEN (m_new.nombre = 'ENSENADA')
            -- Guanajuato
            WHEN e.id = 11 AND (h.id % 4 = 0) THEN (m_new.nombre = 'LEÓN')
            WHEN e.id = 11 AND (h.id % 4 = 1) THEN (m_new.nombre = 'CELAYA')
            WHEN e.id = 11 AND (h.id % 4 = 2) THEN (m_new.nombre = 'IRAPUATO')
            WHEN e.id = 11 THEN (m_new.nombre = 'SALAMANCA')
            -- Chihuahua
            WHEN e.id = 8 AND (h.id % 2 = 0) THEN (m_new.nombre = 'JUÁREZ')
            WHEN e.id = 8 THEN (m_new.nombre = 'CHIHUAHUA')
            -- Jalisco
            WHEN e.id = 14 AND (h.id % 3 = 0) THEN (m_new.nombre = 'GUADALAJARA')
            WHEN e.id = 14 AND (h.id % 3 = 1) THEN (m_new.nombre = 'ZAPOPAN')
            WHEN e.id = 14 THEN (m_new.nombre = 'SAN PEDRO TLAQUEPAQUE')
            -- México (Edomex)
            WHEN e.id = 15 AND (h.id % 3 = 0) THEN (m_new.nombre = 'ECATEPEC DE MORELOS')
            WHEN e.id = 15 AND (h.id % 3 = 1) THEN (m_new.nombre = 'NAUCALPAN DE JUÁREZ')
            WHEN e.id = 15 THEN (m_new.nombre = 'CHIMALHUACÁN')
            -- Michoacán
            WHEN e.id = 16 AND (h.id % 3 = 0) THEN (m_new.nombre = 'MORELIA')
            WHEN e.id = 16 AND (h.id % 3 = 1) THEN (m_new.nombre = 'URUAPAN')
            WHEN e.id = 16 THEN (m_new.nombre = 'ZAMORA')
            -- Nuevo León
            WHEN e.id = 19 AND (h.id % 3 = 0) THEN (m_new.nombre = 'MONTERREY')
            WHEN e.id = 19 AND (h.id % 3 = 1) THEN (m_new.nombre = 'JUÁREZ')
            WHEN e.id = 19 THEN (m_new.nombre = 'GUADALUPE')
            -- Guerrero
            WHEN e.id = 12 AND (h.id % 2 = 0) THEN (m_new.nombre = 'ACAPULCO DE JUÁREZ')
            WHEN e.id = 12 THEN (m_new.nombre = 'CHILPANCINGO DE LOS BRAVO')
            -- Sonora
            WHEN e.id = 26 AND (h.id % 2 = 0) THEN (m_new.nombre = 'CAJEME')
            WHEN e.id = 26 THEN (m_new.nombre = 'HERMOSILLO')
            -- Sinaloa
            WHEN e.id = 25 AND (h.id % 2 = 0) THEN (m_new.nombre = 'CULIACÁN')
            WHEN e.id = 25 THEN (m_new.nombre = 'MAZATLÁN')
            -- CDMX
            WHEN e.id = 9 AND (h.id % 3 = 0) THEN (m_new.nombre = 'IZTAPALAPA')
            WHEN e.id = 9 AND (h.id % 3 = 1) THEN (m_new.nombre = 'GUSTAVO A. MADERO')
            WHEN e.id = 9 THEN (m_new.nombre = 'CUAUHTÉMOC')
            -- Zacatecas
            WHEN e.id = 32 AND (h.id % 2 = 0) THEN (m_new.nombre = 'FRESNILLO')
            WHEN e.id = 32 THEN (m_new.nombre = 'ZACATECAS')
            -- Quintana Roo
            WHEN e.id = 23 AND (h.id % 2 = 0) THEN (m_new.nombre = 'BENITO JUÁREZ')
            WHEN e.id = 23 THEN (m_new.nombre = 'SOLIDARIDAD')
            -- Puebla
            WHEN e.id = 21 THEN (m_new.nombre = 'PUEBLA')
            -- Veracruz
            WHEN e.id = 30 AND (h.id % 2 = 0) THEN (m_new.nombre = 'VERACRUZ')
            WHEN e.id = 30 THEN (m_new.nombre = 'COATZACOALCOS')
            -- Coahuila
            WHEN e.id = 5 THEN (m_new.nombre = 'TORREÓN')
            -- Morelos
            WHEN e.id = 17 THEN (m_new.nombre = 'CUERNAVACA')
            -- San Luis Potosí
            WHEN e.id = 24 THEN (m_new.nombre = 'SAN LUIS POTOSÍ')
            -- Tamaulipas
            WHEN e.id = 28 AND (h.id % 2 = 0) THEN (m_new.nombre = 'REYNOSA')
            WHEN e.id = 28 THEN (m_new.nombre = 'MATAMOROS')
            -- Aguascalientes
            WHEN e.id = 1 THEN (m_new.nombre = 'AGUASCALIENTES')
            -- Tabasco
            WHEN e.id = 27 THEN (m_new.nombre = 'CENTRO')
            -- Colima
            WHEN e.id = 6 THEN (m_new.nombre = 'COLIMA')
            -- Chiapas
            WHEN e.id = 7 THEN (m_new.nombre = 'TUXTLA GUTIÉRREZ')
            -- Oaxaca
            WHEN e.id = 20 THEN (m_new.nombre = 'OAXACA DE JUÁREZ')
            -- Querétaro
            WHEN e.id = 22 THEN (m_new.nombre = 'QUERÉTARO')
            -- Hidalgo
            WHEN e.id = 13 THEN (m_new.nombre = 'PACHUCA DE SOTO')
            -- Durango
            WHEN e.id = 10 THEN (m_new.nombre = 'DURANGO')
            -- Tlaxcala
            WHEN e.id = 29 THEN (m_new.nombre = 'TLAXCALA')
            -- Campeche
            WHEN e.id = 4 THEN (m_new.nombre = 'CAMPECHE')
            -- Yucatán
            WHEN e.id = 31 THEN (m_new.nombre = 'MÉRIDA')
            -- Baja California Sur
            WHEN e.id = 3 THEN (m_new.nombre = 'LA PAZ')
            -- Nayarit
            WHEN e.id = 18 THEN (m_new.nombre = 'TEPIC')
            ELSE 0
        END DESC, m_new.id ASC
    LIMIT 1
)
WHERE m_old.nombre = 'ESTATAL';

-- 2. Actualizar desglose de género coherente y proporcional (10-12% mujeres, 2-3% no identificado, 85-88% hombres)
UPDATE Homicidios
SET 
  mujeres = CASE 
    WHEN num_muertos >= 8 THEN 2
    WHEN num_muertos >= 4 THEN 1
    WHEN num_muertos >= 2 AND (id % 3 = 0) THEN 1
    WHEN num_muertos = 1 AND (id % 8 = 0) THEN 1
    ELSE 0 
  END,
  no_identificado = CASE 
    WHEN num_muertos >= 6 AND (id % 7 = 0) THEN 1
    WHEN num_muertos >= 2 AND (id % 19 = 0) THEN 1
    ELSE 0 
  END;

-- Garantizar la suma exacta: num_muertos = hombres + mujeres + no_identificado
UPDATE Homicidios
SET hombres = num_muertos - mujeres - no_identificado;
