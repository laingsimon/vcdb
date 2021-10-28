DROP USER If Exists MyUser@localhost;
DROP User If Exists MyDisabledUser@localhost;
CREATE USER MyUser@localhost IDENTIFIED BY '123_aBC';
CREATE USER MyDisabledUser@localhost IDENTIFIED BY '123_aBC' ACCOUNT LOCK;
