-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: localhost    Database: bat_calls
-- ------------------------------------------------------
-- Server version	8.0.43

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `calls`
--

DROP TABLE IF EXISTS `calls`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `calls` (
  `id` varchar(45) NOT NULL,
  `FileId` varchar(40) NOT NULL,
  `CallNr` int NOT NULL,
  `FreqMin` float NOT NULL,
  `FreqMax` float NOT NULL,
  `FreqMaxAmp` float NOT NULL,
  `DurationCall` float NOT NULL,
  `StartTime` float NOT NULL,
  `CallInterval` float NOT NULL,
  `Bandwidth` float NOT NULL,
  `SNR` float NOT NULL,
  `SpeciesAuto` varchar(35) NOT NULL,
  `Probability` float NOT NULL,
  `SpeciesMan` varchar(20) NOT NULL,
  `Remarks` varchar(45) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `files`
--

DROP TABLE IF EXISTS `files`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `files` (
  `id` varchar(40) NOT NULL,
  `ProjectId` varchar(30) NOT NULL,
  `WavFileName` varchar(25) NOT NULL,
  `RecordingTime` datetime DEFAULT NULL,
  `Latitude` float NOT NULL,
  `Longitude` float NOT NULL,
  `Temperature` float NOT NULL,
  `Humidity` float NOT NULL,
  `SamplingRate` int NOT NULL,
  `FileLength` float NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `projects`
--

DROP TABLE IF EXISTS `projects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `projects` (
  `id` varchar(30) NOT NULL,
  `Date` date NOT NULL,
  `RecordingDevice` varchar(20) NOT NULL,
  `SwVersion` varchar(20) NOT NULL,
  `PrjCreator` varchar(30) NOT NULL,
  `PathToWavs` varchar(255) NOT NULL,
  `MicrophoneId` varchar(30) NOT NULL,
  `Classifier` varchar(30) NOT NULL,
  `Model` varchar(30) NOT NULL,
  `Location` varchar(80) NOT NULL,
  `Notes` varchar(128) NOT NULL,
  `TriggerSettings` varchar(255) NOT NULL DEFAULT 'unknown',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `species`
--

DROP TABLE IF EXISTS `species`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `species` (
  `latin` varchar(45) NOT NULL,
  `gerrman` varchar(45) DEFAULT NULL,
  `english` varchar(45) DEFAULT NULL,
  `speciescol` varchar(45) DEFAULT NULL,
  `abbreviation` varchar(45) DEFAULT NULL,
  `genus` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`latin`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-10-25  0:24:51
