using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAS.VO;
using System.Web.Mvc;

namespace MSAS.DAO
{
    public class dbAccess
    {
        MSAS_NewEntities db = new MSAS_NewEntities();

        DBEntity d = new DBEntity();
        public AppUsers Login(string email, string password, out string message)
        {

            //
            message = string.Empty;
            MSAS.VO.AppUsers appuser = new VO.AppUsers();
            try
            {

                //  List<User> us = (from o in db.Users.Where(o => o.UserName == email && o.Password == password) select o).ToList();
                User usr = db.Users.Single(o => o.UserName == email && o.Password == password);

                if (usr != null)
                {
                    if (usr.IsActive == false)
                    {
                        message = MSAS.VO.Messages.FILED_USER_LOGIN_ACTIVE_RESTRICTION;

                    }
                    else if (usr.IsDeleted == true)
                    {
                        message = MSAS.VO.Messages.FILED_USER_LOGIN_DELETE_RESTRICTION;

                    }
                    else
                    {
                        if (usr.RoleId == 5)
                        {
                            message = MSAS.VO.Messages.FILED_USER_LOGIN_RESTRICTION;

                        }
                        else
                        {
                            appuser.UserID = usr.UserId;
                            appuser.RoleID = usr.RoleId;
                            appuser.UserEmail = usr.UserName;
                            appuser.Password = usr.Password;

                            UserProfile usrp = db.UserProfiles.Where(o => o.UserId == usr.UserId).First();
                            if (usrp != null)
                            {

                                appuser.UserConId = usrp.UserConId;
                                appuser.FirstName = usrp.FirstName;
                                appuser.MiddleName = usrp.MiddleName;
                                appuser.LastName = usrp.LastName;
                                appuser.Mobile = usrp.Mobile;
                                appuser.PhoneNo = usrp.PhoneNo;
                                appuser.City = usrp.City;
                                appuser.State = usrp.State;
                                appuser.Country = usrp.Country;
                                appuser.Email = usrp.Email;
                                appuser.Address = usrp.Address;
                                appuser.IsPermitted = usrp.ISPermitted;

                                switch (appuser.RoleID)
                                {
                                    case (int)AppUsers.Role.SuperAdmin:
                                        {
                                            GetSuperAdmin(appuser, out message);

                                            break;
                                        }
                                    case (int)AppUsers.Role.AgencyAdmin:
                                        {
                                            GetAdmin(appuser, out message);
                                            break;
                                        }
                                    case (int)AppUsers.Role.AgencyMgr:
                                        {
                                            GetAdmin(appuser, out message);
                                            break;
                                        }
                                    case (int)AppUsers.Role.ClientMgr:
                                        {
                                            GetClient(appuser, out message);

                                            break;
                                        }
                                    case (int)AppUsers.Role.FieldUser:
                                        {
                                            GetClient(appuser, out message);
                                            break;
                                        }
                                    case (int)AppUsers.Role.AgencyStaff:
                                        {
                                            GetAdmin(appuser, out message);
                                            break;
                                        }
                                    case (int)AppUsers.Role.ThirdParty:
                                        {
                                            GetClient(appuser, out message);
                                            break;
                                        }
                                }

                            }
                        }



                    }



                }
                else
                {
                    appuser = null;

                }
            }
            catch (Exception ex)
            {
                //message = GlobalVars.DB_ERROR;
                message = ex.Message;
                appuser = null;
            }
            return appuser;
        }

        public void GetAdmin(MSAS.VO.AppUsers appuser, out string message)
        {
            message = string.Empty;
            MSAS.VO.Agency Agency = new MSAS.VO.Agency();

            //Agency s = (from o in db.Agencies.Where(o => o.AgencyId == appuser.UserConId) select o).SingleOrDefault();
            var s = (from op in db.Agencies
                     join ec in db.EntityCodes on op.AgencyId equals ec.ConId
                     where (ec.EntityCodeTypeId == 1 && op.AgencyId == appuser.UserConId)
                     select op).SingleOrDefault();


            if (s != null)
            {
                if (s.IsActive == false)
                {
                    message = MSAS.VO.Messages.FILED_AGENCY_LOGIN_ACTIVE_RESTRICTION;
                    return;
                }

                if (s.IsDeleted == true)
                {
                    message = MSAS.VO.Messages.FILED_AGENCY_LOGIN_DELETE_RESTRICTION;
                    return;
                }

                Agency.AgencyId = s.AgencyId;
                Agency.PackageId = s.PackageId;
                Agency.AgencyName = s.AgencyName;
                // Agency.SubscriptionDate = (DateTime)s.SubscriptionDate;
                // Agency.ExpiryDate = (DateTime)s.ExpiryDate;
                Agency.BizPOCFirstName = s.BizPOCFirstName;
                Agency.BizPOCLastName = s.BizPOCLastName;
                Agency.BizPOCMobile = s.BizPOCLastName;
                Agency.BizPOCPhone = s.BizPOCPhone;
                Agency.TechPOCFirstName = s.TechPOCFirstName;
                Agency.TechPOCLastName = s.TechPOCLastName;
                Agency.TechPOCPhone = s.TechPOCPhone;
                Agency.Email = s.Email;
                Agency.Address = s.Address;
                Agency.City = s.City;
                Agency.State = s.State;
                Agency.Country = s.Country;
                Agency.Remarks = s.Remarks;
                //  Agency.IsActive = (bool)s.IsActive;
                appuser.Agency = Agency;
                var q = db.EntityCodes.Where(o => o.ConId == Agency.AgencyId && o.EntityCodeTypeId == 1).Single();
                appuser.EntityCodeAgencyID = q.EntityCodeId;
                Package p = db.Packages.Where(o => o.PackageId == appuser.Agency.PackageId && ((o.IsDeleted != true) || (o.IsDeleted == null))).SingleOrDefault();
                if (p != null)
                {
                    PackageProfile pf = db.PackageProfiles.Where(o => o.PackageId == p.PackageId).First();
                    if (pf != null)
                    {
                        appuser.packageList = new VO.Pacakage();
                        appuser.packageList.PackageName = p.PackageName;
                        appuser.packageList.PackageId = p.PackageId;
                        appuser.packageList.TotalClients = (int)pf.TotalClients;
                        appuser.packageList.TotalProjects = (int)pf.TotalProjects;
                        appuser.packageList.TotalUsers = (int)pf.TotalUsers;
                        appuser.packageList.TotalCustomerSurvey = (int)pf.TotalCustomerSurvey;
                        appuser.packageList.TotalProductSurvey = (int)pf.TotalProductSurvey;
                    }
                }


                List<MSAS.VO.Client> GetClient = new List<MSAS.VO.Client>();
                //List<Client> client = (from o in db.Clients.Where(o => o.AgencyId == appuser.Agency.AgencyId && ((o.IsDeleted != true) || (o.IsDeleted == null)) && ((o.IsActive != false) || (o.IsActive == null))) select o).ToList();

                var client = (from op in db.Clients
                              join ec in db.EntityCodes on op.ClientId equals ec.ConId
                              where ((op.IsDeleted == false || op.IsDeleted == null) && ec.EntityCodeTypeId == 2 && op.AgencyId == appuser.Agency.AgencyId && (op.IsActive != false || op.IsActive == null))
                              select op).ToList();
                if (client != null && client.Count != 0)
                {
                    appuser.CurrentClient = new VO.Client();
                    appuser.CurrentClient.ClientId = client[0].ClientId;
                    appuser.CurrentClient.ClientName = client[0].Name;
                    appuser.CurrentClient.Address = client[0].Address;
                    appuser.CurrentClient.AgencyId = client[0].AgencyId;
                    appuser.CurrentClient.BizPOCFirstName = client[0].BizPOCFirstName;
                    appuser.CurrentClient.BizPOCLastName = client[0].BizPOCLastName;
                    appuser.CurrentClient.BizPOCMobile = client[0].BizPOCMobile;
                    appuser.CurrentClient.BizPOCPhone = client[0].BizPOCPhone;
                    appuser.CurrentClient.Email = client[0].Email;
                    appuser.CurrentClient.Address = client[0].Address;
                    appuser.CurrentClient.City = client[0].City;
                    appuser.CurrentClient.LGA = client[0].LGA;
                    appuser.CurrentClient.State = client[0].State;
                    appuser.CurrentClient.Country = client[0].Country;
                    appuser.CurrentClient.Remarks = client[0].Remarks;
                    appuser.CurrentClient.State = client[0].State;
                    appuser.CurrentClient.IsActive = (bool)client[0].IsActive;

                    foreach (var cl in client)
                    {
                        MSAS.VO.Client Clients = new VO.Client();
                        Clients.ClientId = cl.ClientId;
                        Clients.AgencyId = cl.AgencyId;
                        Clients.ClientName = cl.Name;
                        Clients.BizPOCFirstName = cl.BizPOCFirstName;
                        Clients.BizPOCLastName = cl.BizPOCLastName;
                        Clients.BizPOCMobile = cl.BizPOCLastName;
                        Clients.BizPOCPhone = cl.BizPOCPhone;
                        Clients.Email = cl.Email;
                        Clients.Address = cl.Address;
                        Clients.City = cl.City;
                        Clients.LGA = cl.LGA;
                        Clients.State = cl.State;
                        Clients.Country = cl.Country;
                        Clients.Remarks = cl.Remarks;
                        Clients.IsActive = (bool)cl.IsActive;
                        GetClient.Add(Clients);

                    }
                    appuser.Client = GetClient;

                    var CustomerCount = (from op in db.Customers
                                         join pg in db.Users on op.CreatedBy equals pg.UserId
                                         join ec in db.EntityCodes on op.CustomerId equals ec.ConId
                                         where ((op.IsDeleted == false || op.IsDeleted == null) && ec.EntityCodeTypeId == 3 && (op.IsActive != false || op.IsActive == null) && op.ClientId == appuser.CurrentClient.ClientId)
                                         select op).ToList();
                    if (CustomerCount != null && CustomerCount.Count != 0)
                    {
                        appuser.CustomerCount = CustomerCount.Count;
                    }
                    else
                    {
                        appuser.CustomerCount = 0;

                    }

                    var DistributorCount = (from op in db.Distributors
                                            join pg in db.Users on op.CreatedBy equals pg.UserId
                                            join ec in db.EntityCodes on op.DistributorId equals ec.ConId
                                            where ((op.IsDeleted == false || op.IsDeleted == null) && ec.EntityCodeTypeId == 4 && (op.IsActive != false || op.IsActive == null) && op.ClientId == appuser.CurrentClient.ClientId)
                                            select op).ToList();

                    if (DistributorCount != null && DistributorCount.Count != 0)
                    {
                        appuser.DistributorsCount = DistributorCount.Count;
                    }
                    else
                    {

                        appuser.DistributorsCount = 0;
                    }

                    List<UserProfile> UserCount = (from o in db.UserProfiles.Where(o => o.UserConId == appuser.CurrentClient.ClientId && ((o.IsDeleted != true) || (o.IsDeleted == null))) select o).ToList();
                    if (UserCount != null && UserCount.Count != 0)
                    {
                        appuser.UserCount = UserCount.Count;
                    }
                    else
                    {
                        appuser.UserCount = 0;

                    }
                    EntityCode ec1 = db.EntityCodes.Where(o => o.ConId == appuser.CurrentClient.ClientId && o.EntityCodeTypeId == 2).SingleOrDefault();
                    if (ec1 != null)
                    {
                        appuser.EntityCodeID = ec1.EntityCodeId;
                    }

                    EntityCode ec2 = db.EntityCodes.Where(o => o.ConId == appuser.Agency.AgencyId && o.EntityCodeTypeId == 1).SingleOrDefault();
                    if (ec2 != null)
                    {
                        appuser.EntityCodeAgencyID = ec2.EntityCodeId;
                    }
                }

            }
            var SelectFunctionList = (from op in db.AgencyStaffMasters
                                      join pg in db.Agency_Staffs on op.AgencyStaffId equals pg.AgencyStaffId
                                      where (op.UserId == appuser.UserID)
                                      select new { op.AgencyStaffId, pg.AgencyFunxctionList }).Distinct().ToList();

            appuser.AgencyFunctionList = SelectFunctionList.Select(x => new SelectListItem
            {
                Text = x.AgencyFunxctionList,
                Value = x.AgencyStaffId.ToString()

            }).ToList();

            foreach (var i in appuser.AgencyFunctionList)
            {

                if (Int32.Parse(i.Value) == 1)
                {
                    appuser.ClientList = i.Text;
                }
                if (Int32.Parse(i.Value) == 2)
                {
                    appuser.ClientWareHouseList = i.Text;
                }
                if (Int32.Parse(i.Value) == 3)
                {
                    appuser.DistributorList = i.Text;
                }
                if (Int32.Parse(i.Value) == 4)
                {
                    appuser.DistributorWarehouseList = i.Text;
                }
                if (Int32.Parse(i.Value) == 5)
                {
                    appuser.CustomerList = i.Text;
                }
                if (Int32.Parse(i.Value) == 6)
                {
                    appuser.UserList = i.Text;
                }
                if (Int32.Parse(i.Value) == 7)
                {
                    appuser.UserTrackingList = i.Text;
                }
                if (Int32.Parse(i.Value) == 8)
                {
                    appuser.PackageList = i.Text;
                }
                if (Int32.Parse(i.Value) == 9)
                {
                    appuser.ProjectList = i.Text;
                }
                if (Int32.Parse(i.Value) == 10)
                {
                    appuser.GiftList = i.Text;
                }
                if (Int32.Parse(i.Value) == 11)
                {
                    appuser.ProductList = i.Text;
                }
                if (Int32.Parse(i.Value) == 12)
                {
                    appuser.CustomerSurveyList = i.Text;
                }
                if (Int32.Parse(i.Value) == 13)
                {
                    appuser.CustomerReportList = i.Text;
                }
                if (Int32.Parse(i.Value) == 14)
                {
                    appuser.ProductSurveyList = i.Text;
                }
                if (Int32.Parse(i.Value) == 15)
                {
                    appuser.ProductReportList = i.Text;
                }
                if (Int32.Parse(i.Value) == 16)
                {
                    appuser.SaleReportList = i.Text;
                }
                if (Int32.Parse(i.Value) == 17)
                {
                    appuser.SaleDetailsReportlist = i.Text;
                }
                if (Int32.Parse(i.Value) == 18)
                {
                    appuser.CreditSalesReportlist = i.Text;
                }
                if (Int32.Parse(i.Value) == 19)
                {
                    appuser.pointSaleReportList = i.Text;
                }
                if (Int32.Parse(i.Value) == 20)
                {
                    appuser.salescallvisitList = i.Text;
                }
                if (Int32.Parse(i.Value) == 21)
                {
                    appuser.fielduserstockpositionlist = i.Text;
                }
                if (Int32.Parse(i.Value) == 22)
                {
                    appuser.CustomerLoyaltyReportList = i.Text;
                }
                if (Int32.Parse(i.Value) == 23)
                {
                    appuser.IssuedSampleReportList = i.Text;

                }
            }


        }

        public void GetClient(MSAS.VO.AppUsers appuser, out string message)
        {
            message = string.Empty;
            MSAS.VO.Agency Agency = new MSAS.VO.Agency();
            //Client client = (from o in db.Clients.Where(o => o.ClientId == appuser.UserConId) select o).SingleOrDefault();

            var client = (from op in db.Clients
                          join ec in db.EntityCodes on op.ClientId equals ec.ConId
                          where (ec.EntityCodeTypeId == 2 && op.ClientId == appuser.UserConId)
                          select op).SingleOrDefault();
            if (client != null)
            {
                if (client.IsActive == false)
                {
                    message = MSAS.VO.Messages.FILED_ClIENT_LOGIN_ACTIVE_RESTRICTION;
                    return;
                }
                if (client.IsDeleted == true)
                {
                    message = MSAS.VO.Messages.FILED_ClIENT_LOGIN_DELETE_RESTRICTION;
                    return;
                }
                appuser.CurrentClient = new VO.Client();
                appuser.CurrentClient.ClientId = client.ClientId;
                appuser.CurrentClient.ClientName = client.Name;
                appuser.CurrentClient.AgencyId = client.AgencyId;
                appuser.CurrentClient.BizPOCFirstName = client.BizPOCFirstName;
                appuser.CurrentClient.BizPOCLastName = client.BizPOCLastName;
                appuser.CurrentClient.BizPOCMobile = client.BizPOCLastName;
                appuser.CurrentClient.BizPOCPhone = client.BizPOCPhone;
                appuser.CurrentClient.Email = client.Email;
                appuser.CurrentClient.Address = client.Address;
                appuser.CurrentClient.City = client.City;
                appuser.CurrentClient.LGA = client.LGA;
                appuser.CurrentClient.State = client.State;
                appuser.CurrentClient.Country = client.Country;
                appuser.CurrentClient.Remarks = client.Remarks;
                appuser.CurrentClient.IsActive = (bool)client.IsActive;

                //Agency s = (from o in db.Agencies.Where(o => o.AgencyId == client.AgencyId && ((o.IsDeleted != true) || (o.IsDeleted == null)) && ((o.IsActive != false) || (o.IsActive == null))) select o).SingleOrDefault();

                var s = (from op in db.Agencies
                         join ec in db.EntityCodes on op.AgencyId equals ec.ConId
                         where ((op.IsDeleted == false || op.IsDeleted == null) && ec.EntityCodeTypeId == 1 && (op.IsActive != false || op.IsActive == null) && op.AgencyId == client.AgencyId)
                         select op).SingleOrDefault();

                if (s != null)
                {
                    //if (s.IsActive != false)
                    //{
                    //    message = "Your Agency is blocked";
                    //    return;
                    //}
                    //if (client.IsDeleted != true)
                    //{
                    //    message = "Your Agency is deleted";
                    //    return;
                    //}
                    Agency.AgencyId = s.AgencyId;
                    Agency.PackageId = s.PackageId;
                    Agency.AgencyName = s.AgencyName;
                    // Agency.SubscriptionDate = (DateTime)s.SubscriptionDate;
                    // Agency.ExpiryDate = (DateTime)s.ExpiryDate;
                    Agency.BizPOCFirstName = s.BizPOCFirstName;
                    Agency.BizPOCLastName = s.BizPOCLastName;
                    Agency.BizPOCMobile = s.BizPOCLastName;
                    Agency.BizPOCPhone = s.BizPOCPhone;
                    Agency.TechPOCFirstName = s.TechPOCFirstName;
                    Agency.TechPOCLastName = s.TechPOCLastName;
                    Agency.TechPOCPhone = s.TechPOCPhone;
                    Agency.Email = s.Email;
                    Agency.Address = s.Address;
                    Agency.City = s.City;
                    Agency.State = s.State;
                    Agency.Country = s.Country;
                    Agency.Remarks = s.Remarks;
                    //  Agency.IsActive = (bool)s.IsActive;
                    appuser.Agency = Agency;
                }

                var CustomerCount = (from op in db.Customers
                                     join pg in db.Users on op.CreatedBy equals pg.UserId
                                     join ec in db.EntityCodes on op.CustomerId equals ec.ConId
                                     where ((op.IsDeleted == false || op.IsDeleted == null) && ec.EntityCodeTypeId == 3 && (op.IsActive != false || op.IsActive == null) && op.ClientId == appuser.CurrentClient.ClientId)
                                     select op).ToList();
                if (CustomerCount != null && CustomerCount.Count != 0)
                {
                    appuser.CustomerCount = CustomerCount.Count;
                }
                else
                {
                    appuser.CustomerCount = 0;
                }
                //List<Distributor> DistributorCount = (from o in db.Distributors.Where(o => o.ClientId == appuser.CurrentClient.ClientId && ((o.IsDeleted != true) || (o.IsDeleted == null)) && ((o.IsActive != false) || (o.IsActive == null))) select o).ToList();

                var DistributorCount = (from op in db.Distributors
                                        join pg in db.Users on op.CreatedBy equals pg.UserId
                                        join ec in db.EntityCodes on op.DistributorId equals ec.ConId
                                        where ((op.IsDeleted == false || op.IsDeleted == null) && ec.EntityCodeTypeId == 4 && (op.IsActive != false || op.IsActive == null) && op.ClientId == appuser.CurrentClient.ClientId)
                                        select op).ToList();

                if (DistributorCount != null && DistributorCount.Count != 0)
                {
                    appuser.DistributorsCount = DistributorCount.Count;
                }
                else
                {
                    appuser.DistributorsCount = 0;
                }

                List<UserProfile> UserCount = (from o in db.UserProfiles.Where(o => o.UserConId == appuser.CurrentClient.ClientId && ((o.IsDeleted != true) || (o.IsDeleted == null))) select o).ToList();
                if (UserCount != null && UserCount.Count != 0)
                {
                    appuser.UserCount = UserCount.Count;
                }
                else
                {
                    appuser.UserCount = 0;
                }
                EntityCode ec1 = db.EntityCodes.Where(o => o.ConId == appuser.CurrentClient.ClientId && o.EntityCodeTypeId == 2).SingleOrDefault();
                if (ec1 != null)
                {
                    appuser.EntityCodeID = ec1.EntityCodeId;
                }

                EntityCode ec2 = db.EntityCodes.Where(o => o.ConId == appuser.Agency.AgencyId && o.EntityCodeTypeId == 1).SingleOrDefault();
                if (ec2 != null)
                {
                    appuser.EntityCodeAgencyID = ec2.EntityCodeId;
                }
            }
            var SelectFunctionList = (from op in db.AgencyStaffMasters
                                      join pg in db.Agency_Staffs on op.AgencyStaffId equals pg.AgencyStaffId
                                      where (op.UserId == appuser.UserID)
                                      select new { op.AgencyStaffId, pg.AgencyFunxctionList }).Distinct().ToList();

            appuser.AgencyFunctionList = SelectFunctionList.Select(x => new SelectListItem
            {

                Text = x.AgencyFunxctionList,
                Value = x.AgencyStaffId.ToString(),

            }).ToList();

            foreach (var i in appuser.AgencyFunctionList)
            {

                if (Int32.Parse(i.Value) == 1)
                {
                    appuser.ClientList = i.Text;
                }
                if (Int32.Parse(i.Value) == 2)
                {
                    appuser.ClientWareHouseList = i.Text;
                }
                if (Int32.Parse(i.Value) == 3)
                {
                    appuser.DistributorList = i.Text;
                }
                if (Int32.Parse(i.Value) == 4)
                {
                    appuser.DistributorWarehouseList = i.Text;
                }
                if (Int32.Parse(i.Value) == 5)
                {
                    appuser.CustomerList = i.Text;
                }
                if (Int32.Parse(i.Value) == 6)
                {
                    appuser.UserList = i.Text;
                }
                if (Int32.Parse(i.Value) == 7)
                {
                    appuser.UserTrackingList = i.Text;
                }
                if (Int32.Parse(i.Value) == 8)
                {
                    appuser.PackageList = i.Text;
                }
                if (Int32.Parse(i.Value) == 9)
                {
                    appuser.ProjectList = i.Text;
                }
                if (Int32.Parse(i.Value) == 10)
                {
                    appuser.GiftList = i.Text;
                }
                if (Int32.Parse(i.Value) == 11)
                {
                    appuser.ProductList = i.Text;
                }
                if (Int32.Parse(i.Value) == 12)
                {
                    appuser.CustomerSurveyList = i.Text;
                }
                if (Int32.Parse(i.Value) == 13)
                {
                    appuser.CustomerReportList = i.Text;
                }
                if (Int32.Parse(i.Value) == 14)
                {
                    appuser.ProductSurveyList = i.Text;
                }
                if (Int32.Parse(i.Value) == 15)
                {
                    appuser.ProductReportList = i.Text;
                }
                if (Int32.Parse(i.Value) == 16)
                {
                    appuser.SaleReportList = i.Text;
                }
                if (Int32.Parse(i.Value) == 17)
                {
                    appuser.SaleDetailsReportlist = i.Text;
                }
                if (Int32.Parse(i.Value) == 18)
                {
                    appuser.CreditSalesReportlist = i.Text;
                }
                if (Int32.Parse(i.Value) == 19)
                {
                    appuser.pointSaleReportList = i.Text;
                }
                if (Int32.Parse(i.Value) == 20)
                {
                    appuser.salescallvisitList = i.Text;
                }
                if (Int32.Parse(i.Value) == 21)
                {
                    appuser.fielduserstockpositionlist = i.Text;
                }
                if (Int32.Parse(i.Value) == 22)
                {
                    appuser.CustomerLoyaltyReportList = i.Text;
                }
                if (Int32.Parse(i.Value) == 23)
                {
                    appuser.IssuedSampleReportList = i.Text;

                }

            }
        }

        public void GetSuperAdmin(MSAS.VO.AppUsers appuser, out string message)
        {
            message = string.Empty;
            List<MSAS.VO.Agency> GetAgency = new List<MSAS.VO.Agency>();
            //List<Agency> AgList = (from o in db.Agencies.Where(o => ((o.IsDeleted != true) || (o.IsDeleted == null)) && ((o.IsActive != false) || (o.IsActive == null))) select o).ToList();

            var AgList = (from op in db.Agencies
                          join ec in db.EntityCodes on op.AgencyId equals ec.ConId
                          where ((op.IsDeleted == false || op.IsDeleted == null) && ec.EntityCodeTypeId == 1 && (op.IsActive != false || op.IsActive == null))
                          select op).ToList();

            var ecode = (from op in db.Agencies
                         join ec in db.EntityCodes on op.AgencyId equals ec.ConId
                         where ((op.IsDeleted == false || op.IsDeleted == null) && ec.EntityCodeTypeId == 1 && (op.IsActive != false || op.IsActive == null))
                         select ec).ToList();


            if (AgList != null && AgList.Count != 0)
            {
                appuser.Agency = new VO.Agency();
                appuser.Agency.AgencyId = AgList[0].AgencyId;
                appuser.Agency.PackageId = AgList[0].PackageId;
                appuser.Agency.AgencyName = AgList[0].AgencyName;
                // appuser.Agency.SubscriptionDate = (DateTime)AgList[0].SubscriptionDate;
                // appuser.Agency.ExpiryDate = (DateTime)AgList[0].ExpiryDate;
                appuser.Agency.BizPOCFirstName = AgList[0].BizPOCFirstName;
                appuser.Agency.BizPOCLastName = AgList[0].BizPOCLastName;
                appuser.Agency.BizPOCMobile = AgList[0].BizPOCLastName;
                appuser.Agency.BizPOCPhone = AgList[0].BizPOCPhone;
                appuser.Agency.TechPOCFirstName = AgList[0].TechPOCFirstName;
                appuser.Agency.TechPOCLastName = AgList[0].TechPOCLastName;
                appuser.Agency.TechPOCPhone = AgList[0].TechPOCPhone;
                appuser.Agency.Email = AgList[0].Email;
                appuser.Agency.Address = AgList[0].Address;
                appuser.Agency.City = AgList[0].City;
                appuser.Agency.State = AgList[0].State;
                appuser.Agency.Country = AgList[0].Country;
                appuser.Agency.Remarks = AgList[0].Remarks;
                //  a.IsActive = (bool)ag.IsActive;z

                appuser.EntityCodeAgencyID = ecode[0].EntityCodeId;


                foreach (var ag in AgList)
                {

                    MSAS.VO.Agency a = new VO.Agency();
                    a.AgencyId = ag.AgencyId;
                    a.PackageId = ag.PackageId;
                    a.AgencyName = ag.AgencyName;
                    // a.SubscriptionDate = (DateTime)ag.SubscriptionDate;
                    // a.ExpiryDate = (DateTime)ag.ExpiryDate;
                    a.BizPOCFirstName = ag.BizPOCFirstName;
                    a.BizPOCLastName = ag.BizPOCLastName;
                    a.BizPOCMobile = ag.BizPOCLastName;
                    a.BizPOCPhone = ag.BizPOCPhone;
                    a.TechPOCFirstName = ag.TechPOCFirstName;
                    a.TechPOCLastName = ag.TechPOCLastName;
                    a.TechPOCPhone = ag.TechPOCPhone;
                    a.Email = ag.Email;
                    a.Address = ag.Address;
                    a.City = ag.City;
                    a.State = ag.State;
                    a.Country = ag.Country;
                    a.Remarks = ag.Remarks;
                    //  a.IsActive = (bool)ag.IsActive;
                    GetAgency.Add(a);
                }
                appuser.AgencyList = GetAgency;
                Package p = db.Packages.Where(o => o.PackageId == appuser.Agency.PackageId && ((o.IsDeleted != true) || (o.IsDeleted == null))).SingleOrDefault();
                if (p != null)
                {
                    PackageProfile pf = db.PackageProfiles.Where(o => o.PackageId == p.PackageId).First();
                    if (pf != null)
                    {
                        appuser.packageList = new VO.Pacakage();
                        appuser.packageList.PackageName = p.PackageName;
                        appuser.packageList.PackageId = p.PackageId;
                        appuser.packageList.TotalClients = (int)pf.TotalClients;
                        appuser.packageList.TotalProjects = (int)pf.TotalProjects;
                        appuser.packageList.TotalUsers = (int)pf.TotalUsers;
                        appuser.packageList.TotalCustomerSurvey = (int)pf.TotalCustomerSurvey;
                        appuser.packageList.TotalProductSurvey = (int)pf.TotalProductSurvey;
                    }
                }

                List<MSAS.VO.Client> GetClient = new List<MSAS.VO.Client>();
                //List<Client> client = (from o in db.Clients.Where(o => o.AgencyId == appuser.Agency.AgencyId && ((o.IsDeleted != true) || (o.IsDeleted == null)) && ((o.IsActive != false) || (o.IsActive == null))) select o).ToList();

                var client = (from op in db.Clients
                              join ec in db.EntityCodes on op.ClientId equals ec.ConId
                              where ((op.IsDeleted == false || op.IsDeleted == null) && ec.EntityCodeTypeId == 2 && op.AgencyId == appuser.Agency.AgencyId && (op.IsActive != false || op.IsActive == null))
                              select op).ToList();

                if (client != null && client.Count != 0)
                {

                    appuser.CurrentClient = new VO.Client();
                    appuser.CurrentClient.ClientName = client[0].Name;
                    appuser.CurrentClient.Address = client[0].Address;
                    appuser.CurrentClient.ClientId = client[0].ClientId;
                    appuser.CurrentClient.AgencyId = client[0].AgencyId;
                    appuser.CurrentClient.BizPOCFirstName = client[0].BizPOCFirstName;
                    appuser.CurrentClient.BizPOCLastName = client[0].BizPOCLastName;
                    appuser.CurrentClient.BizPOCMobile = client[0].BizPOCMobile;
                    appuser.CurrentClient.BizPOCPhone = client[0].BizPOCPhone;
                    appuser.CurrentClient.Email = client[0].Email;
                    appuser.CurrentClient.Address = client[0].Address;
                    appuser.CurrentClient.City = client[0].City;
                    appuser.CurrentClient.LGA = client[0].LGA;
                    appuser.CurrentClient.State = client[0].State;
                    appuser.CurrentClient.Country = client[0].Country;
                    appuser.CurrentClient.Remarks = client[0].Remarks;
                    appuser.CurrentClient.State = client[0].State;
                    appuser.CurrentClient.IsActive = (bool)client[0].IsActive;

                    foreach (var cl in client)
                    {
                        MSAS.VO.Client Clients = new VO.Client();
                        Clients.ClientId = cl.ClientId;
                        Clients.AgencyId = cl.AgencyId;
                        Clients.ClientName = cl.Name;
                        Clients.BizPOCFirstName = cl.BizPOCFirstName;
                        Clients.BizPOCLastName = cl.BizPOCLastName;
                        Clients.BizPOCMobile = cl.BizPOCLastName;
                        Clients.BizPOCPhone = cl.BizPOCPhone;
                        Clients.Email = cl.Email;
                        Clients.Address = cl.Address;
                        Clients.City = cl.City;
                        Clients.LGA = cl.LGA;
                        Clients.State = cl.State;
                        Clients.Country = cl.Country;
                        Clients.Remarks = cl.Remarks;
                        Clients.IsActive = (bool)cl.IsActive;
                        GetClient.Add(Clients);

                    }
                    appuser.Client = GetClient;

                    //List<Customer> CustomerCount = (from o in db.Customers.Where(o => o.ClientId == appuser.CurrentClient.ClientId && ((o.IsDeleted != true) || (o.IsDeleted == null)) && ((o.IsActive != false) || (o.IsActive == null))) select o).ToList();

                    var CustomerCount = (from op in db.Customers
                                         join pg in db.Users on op.CreatedBy equals pg.UserId
                                         join ec in db.EntityCodes on op.CustomerId equals ec.ConId
                                         where ((op.IsDeleted == false || op.IsDeleted == null) && ec.EntityCodeTypeId == 3 && (op.IsActive != false || op.IsActive == null) && op.ClientId == appuser.CurrentClient.ClientId)
                                         select op).ToList();
                    if (CustomerCount != null && CustomerCount.Count != 0)
                    {
                        appuser.CustomerCount = CustomerCount.Count;
                    }
                    else
                    {
                        appuser.CustomerCount = 0;
                    }

                    //List<Distributor> DistributorCount = (from o in db.Distributors.Where(o => o.ClientId == appuser.CurrentClient.ClientId && ((o.IsDeleted != true) || (o.IsDeleted == null)) && ((o.IsActive != false) || (o.IsActive == null))) select o).ToList();

                    var DistributorCount = (from op in db.Distributors
                                            join pg in db.Users on op.CreatedBy equals pg.UserId
                                            join ec in db.EntityCodes on op.DistributorId equals ec.ConId
                                            where ((op.IsDeleted == false || op.IsDeleted == null) && ec.EntityCodeTypeId == 4 && (op.IsActive != false || op.IsActive == null) && op.ClientId == appuser.CurrentClient.ClientId)
                                            select op).ToList();

                    if (DistributorCount != null && DistributorCount.Count != 0)
                    {
                        appuser.DistributorsCount = DistributorCount.Count;
                    }
                    else
                    {
                        appuser.DistributorsCount = 0;
                    }
                    List<UserProfile> UserCount = (from o in db.UserProfiles.Where(o => o.UserConId == appuser.CurrentClient.ClientId && ((o.IsDeleted != true) || (o.IsDeleted == null))) select o).ToList();
                    if (UserCount != null && UserCount.Count != 0)
                    {
                        appuser.UserCount = UserCount.Count;
                    }
                    else
                    {
                        appuser.UserCount = 0;
                    }

                    //var clients = (from op in db.Clients
                    //               join ec in db.EntityCodes on op.ClientId equals ec.ConId
                    //               where op.IsDeleted == false && ec.EntityCodeTypeId == 2 && op.ClientId == appuser.CurrentClient.ClientId
                    //               select new { ec.EntityCodeId });
                    //appuser.EntityCodeID = MSAS.VO.GlobalFunc.GenerateEntityCode(2);
                    EntityCode ec1 = db.EntityCodes.Where(o => o.ConId == appuser.CurrentClient.ClientId && o.EntityCodeTypeId == 2).SingleOrDefault();
                    if (ec1 != null)
                    {
                        appuser.EntityCodeID = ec1.EntityCodeId;
                    }

                    EntityCode ec2 = db.EntityCodes.Where(o => o.ConId == appuser.Agency.AgencyId && o.EntityCodeTypeId == 1).SingleOrDefault();
                    if (ec2 != null)
                    {
                        appuser.EntityCodeAgencyID = ec2.EntityCodeId;
                    }

                }

            }

        }
        //public void GetAgencyFunction(MSAS.VO.AppUsers appuser, out string message)
        //{
        //    message = string.Empty;
        //var SelectFunctionList = (from op in db.AgencyStaffMasters
        //                           join pg in db.Agency_Staffs on op.AgencyStaffId equals pg.AgencyStaffId
        //                            where(op.UserId == appuser.UserID)
        //                            select new{op.UserId,pg.AgencyFunxctionList}).Distinct().ToList();

        //appuser.AgencyFunctionList = SelectFunctionList.Select(x => new SelectListItem
        //{
        //    Text = x.AgencyFunxctionList,
        //    Value = x.UserId.ToString()

        //}).ToList();
        //return;
        //}

    }
}
