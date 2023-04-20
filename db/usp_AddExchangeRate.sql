CREATE DEFINER=`root`@`%` PROCEDURE `Personal`.`usp_AddExchangeRate`(
	IN symbol CHAR(3), IN rate float, IN ratedate date)
BEGIN
	INSERT INTO ExchangeRates (Symbol, Rate, RateDate)
	VAlUES (symbol, rate, ratedate)
	ON DUPLICATE KEY UPDATE 
	Rate = rate;
END;
