-- ========================================================
-- Script de Dados de Exemplo (Seed Data)
-- Sefic Partners - Gestão de Clientes e Fornecedores
-- ========================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Partners')
BEGIN
    PRINT 'Tabela Partners não encontrada. Por favor, execute as migrações primeiro.';
END
ELSE
BEGIN
    PRINT 'A inserir dados de exemplo na tabela Partners...';

    -- 1. Cliente: Ana Silva
    IF NOT EXISTS (SELECT 1 FROM Partners WHERE TaxNumber = '001234567')
    BEGIN
        INSERT INTO Partners (Id, Name, TaxNumber, Type, Email, CreditLimit, IsActive, CreatedAt, UpdatedAt)
        VALUES (
            NEWID(),
            'Ana Silva',
            '001234567',
            'CUSTOMER',
            'ana.silva@example.com',
            5000.00,
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    -- 2. Fornecedor: Tech Supplies Lda
    IF NOT EXISTS (SELECT 1 FROM Partners WHERE TaxNumber = '502123456')
    BEGIN
        INSERT INTO Partners (Id, Name, TaxNumber, Type, Email, CreditLimit, IsActive, CreatedAt, UpdatedAt)
        VALUES (
            NEWID(),
            'Tech Supplies Lda',
            '502123456',
            'SUPPLIER',
            'contacto@techsupplies.com',
            50000.00,
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    -- 3. Entidade Mista (BOTH): Global Logística & Distribuição
    IF NOT EXISTS (SELECT 1 FROM Partners WHERE TaxNumber = '509988776')
    BEGIN
        INSERT INTO Partners (Id, Name, TaxNumber, Type, Email, CreditLimit, IsActive, CreatedAt, UpdatedAt)
        VALUES (
            NEWID(),
            'Global Logística & Distribuição',
            '509988776',
            'BOTH',
            'geral@globallog.com',
            120000.00,
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    -- 4. Cliente Inativo: Restaurante O Pescador
    IF NOT EXISTS (SELECT 1 FROM Partners WHERE TaxNumber = '501876543')
    BEGIN
        INSERT INTO Partners (Id, Name, TaxNumber, Type, Email, CreditLimit, IsActive, CreatedAt, UpdatedAt)
        VALUES (
            NEWID(),
            'Restaurante O Pescador',
            '501876543',
            'CUSTOMER',
            NULL,
            0.00,
            0,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    -- 5. Fornecedor: Papelaria Central
    IF NOT EXISTS (SELECT 1 FROM Partners WHERE TaxNumber = '504112233')
    BEGIN
        INSERT INTO Partners (Id, Name, TaxNumber, Type, Email, CreditLimit, IsActive, CreatedAt, UpdatedAt)
        VALUES (
            NEWID(),
            'Papelaria Central',
            '504112233',
            'SUPPLIER',
            'vendas@papelariacentral.pt',
            7500.00,
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    PRINT 'Dados de exemplo inseridos com sucesso!';
END
GO
