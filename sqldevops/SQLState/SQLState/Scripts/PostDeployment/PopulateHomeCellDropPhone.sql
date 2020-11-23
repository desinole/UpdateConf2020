Update Customer
Set HomePhone = c.Phone, CellPhone = C.Phone
from Customer C 
where Id = C.Id

ALTER TABLE Customer
DROP COLUMN Phone