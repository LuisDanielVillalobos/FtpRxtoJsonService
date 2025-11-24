USE [LocalLab2000]
GO

/****** Object:  StoredProcedure [dbo].[usp_InsertWebOrder]    Script Date: 24/11/2025 12:20:37 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Luis Daniel
-- Create date: 28/10/2025
-- Description:	Insert Web Orders
-- =============================================
CREATE PROCEDURE [dbo].[usp_InsertWebOrder] 
    @custnum           INT,                             -- obligatorio
    @ponumber          VARCHAR(25),                     -- obligatorio
    @rsphere           FLOAT = NULL,
    @rcylinder         FLOAT = NULL,
    @raxis             TINYINT = NULL,
    @raddition         FLOAT = NULL,
    @lsphere           FLOAT = NULL,
    @lcylinder         FLOAT = NULL,
    @laxis             TINYINT = NULL,
    @laddition         FLOAT = NULL,
    @rheight           TINYINT = NULL,
    @rdip              DECIMAL(10,2) = NULL,
    @lheight           TINYINT = NULL,
    @ldip              DECIMAL(10,2) = NULL,
    @fardip            DECIMAL(16,2) = NULL,
    @roc               FLOAT = NULL,
    @loc               FLOAT = NULL,
    @a                 FLOAT = NULL,
    @b                 FLOAT = NULL,
    @ed                FLOAT = NULL,
    @bridge            FLOAT = NULL,
    @antireflection    varchar(40) = NULL,
    @material          INT = NULL,
    @design            INT = NULL,
    @tint              INT = NULL,
    @notes             VARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    --IF @ponumber IS NULL OR LTRIM(RTRIM(@ponumber)) = ''
    --BEGIN
        --RAISERROR('El parámetro @ponumber es obligatorio.', 16, 1);
        --RETURN -1;
    --END
	 IF EXISTS(select ponumber from dbo.TblWebOrdersAugen where ponumber = @ponumber)
            BEGIN
	update dbo.TblWebOrdersAugen set process_status = 3
	where ponumber = @ponumber AND process_status = 0
            END

    declare @arcode int
    select @arcode = dbo.ufn_GetARid(@antireflection)
    IF not EXISTS(SELECT color FROM TblColors  WHERE cl_color = @tint)
           BEGIN
            set @tint = 0;
            	END
			

    BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO TblWebOrdersAugen
           (
           [custnum],[ponumber]
           ,[rsphere] ,[rcylinder] ,[raxis] ,[raddition]
           ,[lsphere] ,[lcylinder] ,[laxis] ,[laddition]
           ,[rheight] ,[rdip] ,[lheight] ,[ldip] 
           ,[fardip]--,[roc] ,[loc]
           ,[a],[b],[ed]
           ,[bridge],[ar]
           ,[material],[design]
           ,[tint],[notes]
           )
        VALUES
        (
            @custnum, @ponumber,
            @rsphere, @rcylinder, @raxis, @raddition,
            @lsphere, @lcylinder, @laxis, @laddition,
            @rheight, @rdip, @lheight, @ldip,
            @fardip, --@roc, @loc,
            @a, @b, @ed, @bridge,
            @arcode, @material, @design,
            @tint, @notes
        );

        
        COMMIT TRANSACTION;
        --select * from tblWebOrdersAugen
        --exec usp_InsertWebOrder 1, '1', 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 10, '1','1', 1, 1, 1, 1
        select TOP 2 rsphere rsph, rcylinder rcyl, raxis, raddition radd,  
        lsphere lsph, lcylinder lcyl, laxis, laddition ladd, 
        rheight, lheight, rdip, ldip, 
        a, b, ed, bridge dbl, TblAR.AR, M.material , D.diseno design, 
        C.color lenscolor
        ,CASE process_status
        WHEN 0 THEN 'Not started'
        WHEN 3 THEN 'Cancelled'
            END AS status,
        CONCAT(Day(arrived_date), '/', MONTH(arrived_date), '/',YEAR(arrived_date)) AS entry_date
        from tblWebOrdersAugen wb
        inner join TblAR on wb.ar = TblAR.cl_AR
        inner join TblMaterials M on wb.material = M.cl_mat
        inner join TblColors C on wb.tint = C.cl_color
        inner join TblDesigns D on wb.design = D.cl_diseno
        where ponumber=@ponumber order by seqid DESC;
        
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrNum INT = ERROR_NUMBER();
        RAISERROR('Error en usp_InsertOrder: %d - %s', 16, 1, @ErrNum, @ErrMsg);
    END CATCH
END
GO


