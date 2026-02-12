-- Liefert einen Datensatz pro Art und Tag für einen Aufnahmeort
-- der Aufnahmeort wird über die Variablen Lat und Lon festgelegt
WITH Vars AS (
    SELECT 
        49.753822 AS Lat,          -- geografische Breite Aufnahmeort
        8.632532 AS Lon,           -- geografische Laenge Aufnahmeort
        '2025-02-01' AS StartDate, -- Startzeit
        '2025-12-01' AS EndDate,   -- Endzeit
        0.0009 AS dLat,            -- Größe Toleranzfenster (ca. 100m)
        0.0027 AS dLon             -- Größe Toleranzfenster (ca. 100m bei 50°N)
) 
 
SELECT 
    Date, 
    SpeciesMan, 
    AVG(Latitude) AS Avg_Latitude, 
    AVG(Longitude) AS Avg_Longitude,
    MAX(Probability) AS Max_Probability, -- Höchste Wahrscheinlichkeit des Tages
    COUNT(*) AS Detection_Count          -- Wie oft die Art an dem Tag vorkam
FROM projects 
JOIN files ON projects.id = files.ProjectId 
JOIN calls ON files.id = calls.FileId
CROSS JOIN Vars                          -- Dies verbindet die Variablen mit jedem Datensatz
WHERE 
    Latitude BETWEEN Vars.Lat - Vars.dLat AND Vars.Lat + Vars.dLat
    AND Longitude BETWEEN Vars.Lon - Vars.dLon AND Vars.Lon + Vars.dLon
    AND RecordingTime BETWEEN Vars.StartDate AND Vars.EndDate
GROUP BY 
    Date, 
    SpeciesMan
ORDER BY 
    Date ASC
LIMIT 5000;