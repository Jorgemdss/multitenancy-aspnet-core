-- creat schemas
use mig_sharedTenantDb;
drop database mig_sharedTenantDb;
create database mig_sharedTenantDb;

create schema "java";
create schema "charlie";

select *  from sys.schemas

-- insert data

INSERT INTO dbo.products
VALUES ('dbo product', 'lalal',4,4)

-- copiar estrutura das tabelas SEM dados
Select * Into [java].[Products] From [dbo].[Products] Where 1 = 2
Select * Into [charlie].[Products] From [dbo].[Products] Where 1 = 2

-- 

