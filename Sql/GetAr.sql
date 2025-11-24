USE [LocalLab2000]
GO

/****** Object:  UserDefinedFunction [dbo].[ufn_GetARid]    Script Date: 24/11/2025 12:27:29 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		LuisDaniel
-- Create date: 03/11/2025
-- Description:	Get AR id from AR description
-- =============================================
CREATE FUNCTION [dbo].[ufn_GetARid] 
(
	-- Add the parameters for the function here
	@ardescription varchar(40)
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result int = 0

	-- Add the T-SQL statements to compute the return value here
	SELECT @Result = cl_AR from TblAR where AR=@arDescription
	RETURN @Result

END
GO


