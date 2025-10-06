GRANT CREATE TABLE TO [${user_login}];
GRANT CREATE VIEW TO [${user_login}];
GRANT INSERT, UPDATE, DELETE, SELECT TO [${user_login}];
GRANT ALTER TO [${user_login}];
GRANT EXECUTE TO [${user_login}];
GRANT REFERENCES TO [${user_login}];
GO;
