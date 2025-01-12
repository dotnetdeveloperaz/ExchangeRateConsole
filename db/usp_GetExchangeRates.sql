DELIMITER //
CREATE OR REPLACE PROCEDURE `personal`.`usp_GetExchangeRates`(
	IN startDate varchar(10),
	IN endDate varchar(10),
	IN symbols varchar(15),
	IN baseCurrency varchar(3)
)
BEGIN
	SELECT 
		Symbol
		, Rate 
		, RateDate
	FROM ExchangeRates
	WHERE RateDate >= startDate
	AND RateDate <= endDate
	AND FIND_IN_SET(Symbol, symbols) > 0
	AND BaseSymbol = baseCurrency;
END;
//