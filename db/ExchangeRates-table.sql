-- Personal.ExchangeRates definition

CREATE TABLE `ExchangeRates` (
  `Id` mediumint(9) NOT NULL AUTO_INCREMENT,
  `Symbol` char(3) NOT NULL,
  `BaseSymbol` char(3) NOT NULL,
  `Rate` float NOT NULL,
  `RateDate` date NOT NULL,
  `AddDate` timestamp NOT NULL DEFAULT current_timestamp(),
  `Last_Update` timestamp NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `ExchangeRates_UN` (`Symbol`, `BaseSymbol`, `RateDate`)
) ENGINE=InnoDB AUTO_INCREMENT=12827 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;