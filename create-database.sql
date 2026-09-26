CREATE DATABASE elasticsearchdemo;

CREATE TABLE  IF NOT EXISTS products (
    Id              integer CONSTRAINT products_pk PRIMARY KEY,
    Name            varchar(200) NOT NULL,
    Description     varchar(1000) NOT NULL,
    CategoryId      integer NOT NULL,
    Price           money NOT NULL,
    Stock           integer NOT NULL,
    CreatedAt       timestamp NOT NULL
);

CREATE TABLE  IF NOT EXISTS categories (
    Id              integer CONSTRAINT categories_pk PRIMARY KEY,
    Name            varchar(100) NOT NULL
);

ALTER TABLE products ADD CONSTRAINT categories_products_fk FOREIGN KEY (CategoryId) REFERENCES categories (Id) MATCH FULL;


DROP TABLE IF EXISTS products;

DROP TABLE IF EXISTS categories;

DROP DATABASE IF EXISTS elasticsearchdemo;
