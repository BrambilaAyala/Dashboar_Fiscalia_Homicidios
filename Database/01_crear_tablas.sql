-- =============================================================================
-- PROYECTO: Dashboard_Fiscalia_Homicidios (ProyectoFisca)
-- SCRIPT: 01_crear_tablas.sql
-- DESCRIPCIÓN: Creación de la base de datos y esquema de tablas para homicidios
-- MOTOR: MySQL 8.0+
-- JUEGO DE CARACTERES: utf8mb4 / utf8mb4_unicode_ci (soporte acentos y caracteres especiales)
-- =============================================================================

-- 1. Crear base de datos
CREATE DATABASE IF NOT EXISTS fiscalia
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE fiscalia;

-- 2. Eliminar tablas previas si existen (orden inverso de dependencias)
DROP TABLE IF EXISTS Homicidios;
DROP TABLE IF EXISTS ReportesInconsistencias;
DROP TABLE IF EXISTS Municipio;
DROP TABLE IF EXISTS Entidad;

-- -----------------------------------------------------------------------------
-- TABLA 1: Entidad (Entidades Federativas de la República Mexicana)
-- -----------------------------------------------------------------------------
CREATE TABLE Entidad (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(150) NOT NULL UNIQUE,
    clave VARCHAR(10) NULL,
    abreviatura VARCHAR(10) NULL,
    fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_entidad_nombre (nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- TABLA 2: Municipio (Municipios y Alcaldías de México)
-- -----------------------------------------------------------------------------
CREATE TABLE Municipio (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(150) NOT NULL,
    entidad_id INT NOT NULL,
    clave VARCHAR(10) NULL,
    fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_municipio_entidad UNIQUE (nombre, entidad_id),
    CONSTRAINT fk_municipio_entidad FOREIGN KEY (entidad_id) 
        REFERENCES Entidad(id) 
        ON DELETE CASCADE 
        ON UPDATE CASCADE,
    INDEX idx_municipio_nombre (nombre),
    INDEX idx_municipio_entidad_id (entidad_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- TABLA 3: Homicidios (Registros diarios de homicidios dolosos)
-- -----------------------------------------------------------------------------
CREATE TABLE Homicidios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    municipio_id INT NOT NULL,
    fecha DATE NOT NULL,
    num_muertos INT NOT NULL DEFAULT 0,
    hombres INT NOT NULL DEFAULT 0,
    mujeres INT NOT NULL DEFAULT 0,
    no_identificado INT NOT NULL DEFAULT 0,
    fuente VARCHAR(255) NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_homicidios_municipio FOREIGN KEY (municipio_id) 
        REFERENCES Municipio(id) 
        ON DELETE CASCADE 
        ON UPDATE CASCADE,
    INDEX idx_homicidios_fecha (fecha),
    INDEX idx_homicidios_municipio_id (municipio_id),
    INDEX idx_homicidios_fecha_municipio (fecha, municipio_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- TABLA 4: ReportesInconsistencias (Auditoría de anomalías en PDFs)
-- -----------------------------------------------------------------------------
CREATE TABLE ReportesInconsistencias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fecha DATETIME NOT NULL,
    entidad_detectada VARCHAR(150) NULL,
    municipio_detectado VARCHAR(150) NULL,
    num_muertos INT NOT NULL DEFAULT 0,
    fuente VARCHAR(255) NULL,
    motivo_inconsistencia TEXT NULL,
    fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_inconsistencias_fecha (fecha)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- VISTAS ANALÍTICAS (Optimizadas para consultas del Dashboard y API)
-- -----------------------------------------------------------------------------

-- Vista 1: Resumen de homicidios agrupado por Entidad Federativa
CREATE OR REPLACE VIEW v_homicidios_resumen_entidad AS
SELECT 
    e.nombre AS entidad,
    SUM(h.num_muertos) AS total_homicidios,
    SUM(h.hombres) AS total_hombres,
    SUM(h.mujeres) AS total_mujeres,
    SUM(h.no_identificado) AS total_no_identificado,
    COUNT(DISTINCT h.municipio_id) AS municipios_con_incidencias
FROM Entidad e
JOIN Municipio m ON e.id = m.entidad_id
JOIN Homicidios h ON m.id = h.municipio_id
GROUP BY e.id, e.nombre
ORDER BY total_homicidios DESC;

-- Vista 2: Agrupación temporal trimestral (Año y Trimestre Q1-Q4)
CREATE OR REPLACE VIEW v_homicidios_trimestrales AS
SELECT 
    YEAR(h.fecha) AS anio,
    QUARTER(h.fecha) AS trimestre_numero,
    CONCAT(
        YEAR(h.fecha), ' - ', 
        CASE 
            WHEN QUARTER(h.fecha) = 1 THEN 'Enero-Marzo'
            WHEN QUARTER(h.fecha) = 2 THEN 'Abril-Junio'
            WHEN QUARTER(h.fecha) = 3 THEN 'Julio-Septiembre'
            WHEN QUARTER(h.fecha) = 4 THEN 'Octubre-Diciembre'
        END
    ) AS trimestre,
    SUM(h.num_muertos) AS num_muertos,
    SUM(h.hombres) AS hombres,
    SUM(h.mujeres) AS mujeres,
    SUM(h.no_identificado) AS no_identificado,
    MIN(h.fecha) AS fecha_representativa
FROM Homicidios h
GROUP BY YEAR(h.fecha), QUARTER(h.fecha), trimestre
ORDER BY anio ASC, trimestre_numero ASC;
