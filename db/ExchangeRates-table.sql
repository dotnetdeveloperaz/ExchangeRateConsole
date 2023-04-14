-- Personal.ExchangeRates definition

CREATE TABLE `ExchangeRates` (
  `Id` mediumint(9) NOT NULL AUTO_INCREMENT,
  `Symbol` char(3) NOT NULL,
  `Rate` float NOT NULL,
  `RateDate` date NOT NULL,
  `AddDate` timestamp NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4655 DEFAULT CHARSET=utf8mb4;
