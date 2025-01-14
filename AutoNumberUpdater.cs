using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Logging;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Args;
using XrmToolBox.Extensibility.Interfaces;
using Microsoft.Xrm.Sdk.Organization;
using System.Web.Services.Description;

namespace Sdmsols.XTB.AutoNumberUpdater
{
    public partial class AutoNumberUpdater : PluginControlBase,IGitHubPlugin, IPayPalPlugin, IMessageBusHost, IHelpPlugin, IStatusBarMessenger, IAboutPlugin
    {
        #region Constructor and Class Variables

        private Settings _mySettings;
        private List<EntityMetadataProxy> _entities;
        
        private EntityMetadataProxy _selectedEntity;

        private AttributeProxy _selectedAttributeMetadata;

        private int _stateCode = -1;

        public event EventHandler<MessageBusEventArgs> OnOutgoingMessage;
        public event EventHandler<StatusBarMessageEventArgs> SendMessageToStatusBar;


        private enum ControlSelected
        {
            Solutions=1,
            Entities=2,
            Attributes=3,
            StateCodes=4
        }

        public AutoNumberUpdater()
        {
            InitializeComponent();
        }

        #endregion Constructor and Class Variables

        #region XrmToolBox Plug In Methods

        private void AutoNumberUpdater_Load(object sender, EventArgs e)
        {
           // ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out _mySettings))
            {
                _mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (_mySettings != null && detail != null)
            {
                _mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
        }

        private void AutoNumberUpdater_ConnectionUpdated(object sender, ConnectionUpdatedEventArgs e)
        {
            var orgver = new Version(e.ConnectionDetail.OrganizationVersion);
            var orgok = orgver >= new Version(9, 0);

            if (orgok)
            {
                LoadSolutions();
                LoadEntities();
            }
            else
            {
                LogError("CRM version too old for Auto Number Manager");

                MessageBox.Show($"Auto Number feature was introduced in\nMicrosoft Dynamics 365 July 2017 (9.0)\nCurrent version is {orgver}\n\nPlease connect to a newer organization to use this cool tool.",
                    "Organization too old", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoNumberUpdater_OnCloseTool(object sender, System.EventArgs e)
        {

            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), _mySettings);
        }



        #endregion XrmToolBox Plug In Methods
        
        #region Control Events

        private void cmbSolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisableControls((int)ControlSelected.Solutions);
            if (cmbSolution.SelectedItem != null)
            {
                FilterEntities();
            }
        }

        private void cmbEntities_SelectedIndexChanged(object sender, EventArgs e)
        {

            DisableControls((int)ControlSelected.Entities);
            if (cmbEntities.SelectedItem != null)
            {
                _selectedEntity = (EntityMetadataProxy) cmbEntities.SelectedItem;

                LoadStateCodes();
                LoadAttributes(false);
            }
        }

        private void cmbStateCodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisableControls((int)ControlSelected.StateCodes);
            if (cmbStateCodes.SelectedValue != null && cmbStateCodes.SelectedValue is int)
            {
                _stateCode = (int)cmbStateCodes.SelectedValue;
            }
        }

        private void cmbAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisableControls((int)ControlSelected.Attributes);

            if (cmbAttributes.SelectedItem != null)
            {
                _selectedAttributeMetadata = (AttributeProxy) cmbAttributes.SelectedItem;

                if (_selectedAttributeMetadata != null)
                {
                    var selectedFormat = _selectedAttributeMetadata.attributeMetadata.AutoNumberFormat;
                    int nextValue = GuessSeed();
                    txtSample.Text = ParseNumberFormat(selectedFormat, nextValue.ToString());

                    if (!string.IsNullOrEmpty(txtSample.Text))
                    {
                        btnFixAutoNumbers.Enabled = true;
                    }
                }
            }



        }

        private void btnFixAutoNumbers_Click(object sender, EventArgs e)
        {
            try
            {

                ExecuteMethod(GetRecordsAndFixAutoNumbers);
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"an Error has occurred while processing.." + exception.Message);
            }
           
        }

        #endregion Control Events
        
        #region Private Helper Methods

        private void LoadSolutions()
        {
            cmbSolution.Items.Clear();
            cmbSolution.Enabled = false;
            WorkAsync(new WorkAsyncInfo("Loading solutions...",
                (eventargs) =>
                {
                    var qx = new QueryExpression("solution");
                    qx.ColumnSet.AddColumns("friendlyname", "uniquename");
                    qx.AddOrder("installedon", OrderType.Ascending);
                    //qx.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
                    qx.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);
                    var lePub = qx.AddLink("publisher", "publisherid", "publisherid");
                    lePub.EntityAlias = "P";
                    lePub.Columns.AddColumns("customizationprefix");
                    eventargs.Result = Service.RetrieveMultiple(qx);
                })
            {
                PostWorkCallBack = (completedargs) =>
                {
                    if (completedargs.Error != null)
                    {
                        MessageBox.Show(completedargs.Error.Message);
                    }
                    else
                    {
                        if (completedargs.Result is EntityCollection)
                        {
                            var solutions = (EntityCollection)completedargs.Result;
                            var proxiedsolutions = solutions.Entities.Select(s => new SolutionProxy(s)).OrderBy(s => s.ToString());
                            cmbSolution.Items.AddRange(proxiedsolutions.ToArray());
                            cmbSolution.Enabled = true;
                        }
                    }
                    
                }
            });
        }

        private void LoadEntities()
        {
            _entities = new List<EntityMetadataProxy>();
            WorkAsync(new WorkAsyncInfo("Loading entities...",
                (eventargs) => { eventargs.Result = MetadataHelper.LoadEntities(Service); })
            {
                PostWorkCallBack = (completedargs) =>
                {
                    if (completedargs.Error != null)
                    {
                        MessageBox.Show(completedargs.Error.Message);
                    }
                    else
                    {
                        if (completedargs.Result is RetrieveMetadataChangesResponse)
                        {
                            var metaresponse = ((RetrieveMetadataChangesResponse)completedargs.Result).EntityMetadata;
                            _entities.AddRange(metaresponse
                                .Where(e => e.IsCustomizable.Value == true && e.IsIntersect.Value != true)
                                .Select(m => new EntityMetadataProxy(m))
                                .OrderBy(e => e.ToString()));
                        }
                    }

                }
            });
        }

        private void FilterEntities()
        {
            cmbEntities.Items.Clear();
            cmbEntities.Enabled = false;

            var solution = cmbSolution.SelectedItem as SolutionProxy;
            if (solution == null)
            {
                return;
            }
            
            WorkAsync(new WorkAsyncInfo("Filtering entities...",
                (eventargs) =>
                {
                    
                    var qx = new QueryExpression("solutioncomponent");
                    qx.ColumnSet.AddColumns("objectid");
                    qx.Criteria.AddCondition("componenttype", ConditionOperator.Equal, 1);
                    qx.Criteria.AddCondition("solutionid", ConditionOperator.Equal, solution.Solution.Id);
                    eventargs.Result = Service.RetrieveMultiple(qx);
                })
            {
                PostWorkCallBack = (completedargs) =>
                {
                    if (completedargs.Error != null)
                    {
                        MessageBox.Show(completedargs.Error.Message);
                    }
                    else
                    {
                        if (completedargs.Result is EntityCollection)
                        {
                            var includedentities = (EntityCollection)completedargs.Result;
                            var filteredentities = _entities.Where(e => includedentities.Entities.Select(i => i["objectid"]).Contains(e.Metadata.MetadataId));
                            cmbEntities.Items.AddRange(filteredentities.ToArray());
                        }
                    }
                    cmbEntities.Enabled = true;
                }
            });
        }

        private void LoadStateCodes()
        {
            //cmbStateCodes.Items.Clear();
            cmbStateCodes.Enabled = false;
            var entity = cmbEntities.SelectedItem as EntityMetadataProxy;

            WorkAsync(new WorkAsyncInfo("Filtering entities...",
            (eventargs) =>
            {

                var attributeRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = entity.Metadata.LogicalName,
                    LogicalName = "statecode",
                    RetrieveAsIfPublished = true
                };

                var attributeResponse = (RetrieveAttributeResponse)Service.Execute(attributeRequest);
                var attributeMetadata = (EnumAttributeMetadata)attributeResponse.AttributeMetadata;

                var optionList = (from o in attributeMetadata.OptionSet.Options
                                  select new KeyValuePair<string,int>(o.Label.UserLocalizedLabel.Label, o.Value.Value)).ToList();

                optionList.Insert(0, new KeyValuePair<string, int>("Default (No StateCode Filter)", -1));

                eventargs.Result = optionList;
            })
            {
                PostWorkCallBack = (completedargs) =>
                {
                    if (completedargs.Error != null)
                    {
                        MessageBox.Show(completedargs.Error.Message);
                    }
                    else
                    {
                        if (completedargs.Result is List<KeyValuePair<string, int>>)
                        {
                            var options = (List<KeyValuePair<string, int>>)completedargs.Result;
                            cmbStateCodes.Enabled = true;
                            cmbStateCodes.ValueMember = "Value";
                            cmbStateCodes.DisplayMember = "Key";
                            cmbStateCodes.DataSource = options;
                        }
                    }

                }
            });
        }

        private void LoadAttributes(bool force)
        {
            cmbAttributes.Items.Clear();
            cmbAttributes.Enabled = false;
            var entity = cmbEntities.SelectedItem as EntityMetadataProxy;
            var onlyNumbered = true;
            WorkAsync(new WorkAsyncInfo("Loading auto number attributes...",
                (eventargs) =>
                {
                    if (force || entity.Metadata.Attributes == null)
                    {
                        eventargs.Result = MetadataHelper.LoadEntityDetails(Service, entity.Metadata.LogicalName).EntityMetadata.FirstOrDefault();
                    }
                    else
                    {
                        eventargs.Result = entity.Metadata;
                    }
                })
            {
                PostWorkCallBack = (completedargs) =>
                {
                    if (completedargs.Result is EntityMetadata)
                    {
                        try
                        {
                            entity.Metadata = (EntityMetadata)completedargs.Result;
                            var attributes = entity.Metadata.Attributes
                              .Where(a => a.AttributeType == AttributeTypeCode.String &&
                                  a.IsValidForCreate.Value == true &&
                                  a.IsCustomizable.Value == true &&
                                  (!onlyNumbered || !string.IsNullOrEmpty(a.AutoNumberFormat)))
                              .Select(a => new AttributeProxy((StringAttributeMetadata)a)).OrderBy(a => a.LogicalName).ToList();
                            var bindingList = new BindingList<AttributeProxy>(attributes);
                            var source = new BindingSource(bindingList, null);

                            cmbAttributes.Enabled = true;
                            cmbAttributes.Items.AddRange(bindingList.ToArray());

                        }
                        catch (MissingMethodException mex)
                        {
                            //LogUse("IncompatibleSDK");
                            MessageBox.Show("It seems you are using too old SDK, that is unaware of the AutoNumberFormat property.", "SDK error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            });
        }
        
        private void DisableControls(int controlSelected)
        {
            switch (controlSelected)
            {
                case (int)ControlSelected.Solutions:
                    cmbEntities.Items.Clear();
                    cmbEntities.Enabled = false;
                    cmbAttributes.Items.Clear();
                    cmbAttributes.Enabled = false;
                    cmbStateCodes.DataSource = null;
                    cmbStateCodes.Enabled = false;
                    txtSample.Text = "";
                    btnFixAutoNumbers.Enabled = false;
                    _stateCode = -1;
                    break;
                case (int)ControlSelected.Entities:
                    cmbAttributes.Items.Clear();
                    cmbAttributes.Enabled = false;
                    txtSample.Text = "";
                    btnFixAutoNumbers.Enabled = false;
                    break;
                case (int)ControlSelected.Attributes:
                    txtSample.Text = "";
                    btnFixAutoNumbers.Enabled = false;
                    break;
                case (int)ControlSelected.StateCodes:
                    break;


            }
        }
        
        private void FixEntityAutoNumbers(EntityCollection results)
        {
            var selectedFormat = "";

            var selectedAttribute = _selectedAttributeMetadata.LogicalName;
            selectedFormat = _selectedAttributeMetadata.attributeMetadata.AutoNumberFormat;


            //var selectedFormat
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Started Updating Auto Numbers for " + _selectedEntity.Metadata.LogicalName + " Records which are missing auto numbers..",
                Work = (worker, args) =>
                {
                    UpdateStatusMessage("Started Updating Auto Numbers for " + _selectedEntity.Metadata.LogicalName + " Records which are missing auto numbers..");


                    if (!String.IsNullOrEmpty(selectedFormat))
                    {

                        foreach (var currentEntity in results.Entities)
                        {
                            var primaryName = currentEntity.Contains(_selectedEntity.Metadata.PrimaryNameAttribute) ? currentEntity[_selectedEntity.Metadata.PrimaryNameAttribute].ToString() : "";
                            UpdateStatusMessage($"Started Processing Id: {currentEntity.Id} and {_selectedEntity.Metadata.PrimaryNameAttribute}:{primaryName}.. ");


                            int nextValue = GuessSeed();                          
                            var nextNumber = ParseNumberFormat(selectedFormat, nextValue.ToString());

                            if (!string.IsNullOrEmpty(nextNumber))
                            {
                                Entity updateEntity = new Entity(currentEntity.LogicalName);
                                updateEntity.Id = currentEntity.Id;
                                updateEntity[selectedAttribute] = nextNumber;


                                //only update if this not running under actual mode

                                UpdateStatusMessage(
                                    $" Record Id: {currentEntity.Id} and {_selectedEntity.Metadata.PrimaryNameAttribute}:{primaryName} will be updated with value  {selectedAttribute}:{nextNumber}");

                                Service.Update(updateEntity);

                                //UPDATE SEED TO ENSURE NEXT NUMBER GETS UPDATED!
                                int seedValue = nextValue;
                                OrganizationRequest customActionRequest = new OrganizationRequest("SetAutoNumberSeed");
                                customActionRequest["EntityName"] = currentEntity.LogicalName;
                                customActionRequest["AttributeName"] = selectedAttribute;
                                customActionRequest["Value"] = Convert.ToInt64(nextValue);
                                Service.Execute(customActionRequest);
                            }
                            else
                            {

                                UpdateStatusMessage($"Failed to Find Next number for Processing...");
                                break;
                            }

                            UpdateStatusMessage($"Completed Processing Id: {currentEntity.Id} and {_selectedEntity.Metadata.PrimaryNameAttribute}:{primaryName}.. ");

                            AddProgressStep();
                        }
                    }



                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    UpdateStatusMessage("Finish Updating Auto Numbers for " + _selectedEntity.Metadata.LogicalName + " Records which are missing auto numbers..");


                },
                IsCancelable = true
            });

        }
        private EntityCollection GetAllRecordsWithOutAutoNumberPopulated()
        {
            //var selectedAttributeMetadata = (AttributeProxy)cmbAttributes.SelectedItem;
            var selectedAttribute = _selectedAttributeMetadata.LogicalName;

            var results = new EntityCollection();

            var query = new QueryExpression(_selectedEntity.Metadata.LogicalName)
            {
                ColumnSet = new ColumnSet(_selectedEntity.Metadata.LogicalName + "id", _selectedEntity.Metadata.PrimaryNameAttribute, selectedAttribute),
                PageInfo = new PagingInfo
                {
                    PageNumber = 1,
                    PagingCookie = null,
                    Count = 0x1388
                },
                Criteria = new FilterExpression(LogicalOperator.And)
            };
            query.Criteria.AddCondition(selectedAttribute, ConditionOperator.Null);

            if (chkAscending.Checked)
            {
                query.AddOrder(attributeName: txtOrderAttribute.Text, orderType: OrderType.Ascending);
            }
            else
            {
                query.AddOrder(attributeName: txtOrderAttribute.Text, orderType: OrderType.Descending);
            }

            if (_stateCode != -1)
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal,_stateCode);

            while (true)
            {
                var entities = this.Service.RetrieveMultiple(query);
                results.Entities.AddRange(entities.Entities);
                if (entities.MoreRecords)
                {
                    var pageInfo = query.PageInfo;
                    pageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = entities.PagingCookie;
                }
                else
                {
                    break;
                }
            }

            return results;
        }


        private void GetRecordsAndFixAutoNumbers()
        {
           
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting " + _selectedEntity.Metadata.LogicalName + " Records which are missing auto numbers..",
                Work = (worker, args) =>
                {

                    var result = GetAllRecordsWithOutAutoNumberPopulated();
                    args.Result = result;

                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    var result = args.Result as EntityCollection;
                    if (result != null)
                    {
                        //MessageBox.Show($"Found {result.Entities.Count} contacts");
                        LogTextBoxAndProgressBar.UpdateStatusMessage(StatusText, $"Found {result.Entities.Count} {_selectedEntity.Metadata.LogicalName} Records.");
                    }

                    if (result != null)
                        LogTextBoxAndProgressBar.SetProgressBar(progressBar, result.Entities.Count);


                    FixEntityAutoNumbers(result);
                }
            });
        }

        #endregion Private Helper Methods
        
        #region Logging And ProgressBar Methods
        delegate void SetStatusTextCallback(string text);

        delegate void AddProgressStepCallback();

        private void UpdateStatusMessage(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.StatusText.InvokeRequired)
            {
                SetStatusTextCallback d = new SetStatusTextCallback(UpdateStatusMessage);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                //this.StatusText.Text = text;

                StatusText.Text = StatusText.Text + text + Environment.NewLine;

                StatusText.Focus();
                StatusText.ScrollToCaret();
                ErrorLog.ReportRoutine(false, text, EventLogEntryType.Information);

                Application.DoEvents();


            }
        }
        
        private void AddProgressStep()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.progressBar.InvokeRequired)
            {
                AddProgressStepCallback d = new AddProgressStepCallback(AddProgressStep);
                this.Invoke(d);
            }
            else
            {
                progressBar.PerformStep();
                Application.DoEvents();
            }
        }

        #endregion Logging And ProgressBar Methods
        
        #region Auto Number Format Methods

        private int GuessSeed()
        {
            var result = 0;
            var lastvalue = string.Empty;
            //var entity = selectedEntity;
            var entity = _entities.Find(a => a.Metadata.LogicalName.Equals(_selectedEntity.Metadata.LogicalName));
            var selectedAttribute = _selectedAttributeMetadata.LogicalName;
            var attributename = selectedAttribute;

            var autoNumberResult = (GetNextAutoNumberValueResponse)Service.Execute(new GetNextAutoNumberValueRequest()
            {
                EntityName = entity.Metadata.LogicalName,
                AttributeName = attributename
            });


            if (autoNumberResult != null)
            {
                result = (int)autoNumberResult.NextAutoNumberValue;
                lastvalue = autoNumberResult.NextAutoNumberValue.ToString();

            }
            else
            {
                //if Dynamics API does not return latest value then find latest value using manual approach now!
                var selectedFormat = "";

                selectedFormat = _selectedAttributeMetadata.attributeMetadata.AutoNumberFormat;

                var format = selectedFormat;
                var sample = ParseNumberFormat(format, "9999999999");

                if (!format.Contains("{SEQNUM:") || !format.Contains("}"))
                {
                    throw new FormatException("Format string must contain a {SEQNUM:n} placeholder.");
                }
                var seqstart = sample.IndexOf("9999999999");
                var lenghtstr = format.Split(new string[] { "{SEQNUM:" }, StringSplitOptions.None)[1];
                lenghtstr = lenghtstr.Split('}')[0];
                var length = 0;
                if (int.TryParse(lenghtstr, out length))
                {
                    if (length < 1)
                    {
                        throw new FormatException("Failed to parse SEQNUM length.");
                    }
                }

                var fetchxml = "<fetch top='1' ><entity name='" + entity.Metadata.LogicalName + "' >" +
                    "<attribute name='" + attributename + "' />" +
                    "<filter><condition attribute='" + attributename + "' operator='not-null' /></filter>" +
                    "<order attribute='" + selectedAttribute + "' descending='true' /></entity></fetch>";
                var lastrecord = Service.RetrieveMultiple(new FetchExpression(fetchxml)).Entities.FirstOrDefault();
                
                if (lastrecord == null)
                {
                    var seedResult = (GetAutoNumberSeedResponse)Service.Execute(new GetAutoNumberSeedRequest()
                    {
                        EntityName = entity.Metadata.LogicalName,
                        AttributeName = attributename
                    });

                    if (seedResult != null)
                    {
                        return (int)seedResult.AutoNumberSeedValue - 1;
                    }
                    else
                    {
                        //throw new Exception("No numbered data found for attribute " + attributename);
                        return 0;
                    }
                }
                lastvalue = lastrecord[attributename].ToString();
                if (lastvalue.Length >= seqstart + length)
                {
                    var lastseqstr = lastvalue.Substring(seqstart, length);
                    if (int.TryParse(lastseqstr, out int lastseq))
                    {
                        //LogUse("GuessSeed succeeded");
                        result = lastseq;
                    }
                }

            }

            if (result == 0)
            {
                //LogUse("GuessSeed failed");
                throw new Exception("That was hard. Couldn't even guess what current SEQNUM is.\n" +
                    "Numbered value for last created record is:  \n" + lastvalue);
            }
            return result;
        }

        private string ParseNumberFormat(string format, string seed)
        {
           // txtHint.Text = string.Empty;
            if (!string.IsNullOrWhiteSpace(format))
            {
                try
                {
                    format = ParseFormatSEQNUM(format, seed);
                    format = ParseFormatRANDSTRING(format);
                    format = ParseFormatDATETIMEUTC(format);
                    //txtHint.Text = "Format successfully parsed.";
                   
                }
                catch (Exception ex)
                {
                    //txtHint.Text = ex.Message;
                    format = string.Empty;
                    //btnCreateUpdate.Enabled = false;
                }
            }
            return format;
        }

        private string ParseFormatDATETIMEUTC(string format)
        {
            while (format.Contains("{DATETIMEUTC:") && format.Contains("}"))
            {
                var formatstr = format.Split(new string[] { "{DATETIMEUTC:" }, StringSplitOptions.None)[1];
                formatstr = formatstr.Split('}')[0];
                var datestr = DateTime.Now.ToString(formatstr);
                format = format.Replace("{DATETIMEUTC:" + formatstr + "}", datestr);
            }
            return format;
        }

        private string ParseFormatRANDSTRING(string format)
        {
            while (format.Contains("{RANDSTRING:") && format.Contains("}"))
            {
                var lenghtstr = format.Split(new string[] { "{RANDSTRING:" }, StringSplitOptions.None)[1];
                lenghtstr = lenghtstr.Split('}')[0];
                if (int.TryParse(lenghtstr, out int length))
                {
                    if (length < 1 || length > 6)
                    {
                        throw new FormatException("RANDSTRING length must be between 1 and 6");
                    }
                    var randomstring = "X7C7D8EK3MR2L4".Substring(0, length);
                    format = format.Replace("{RANDSTRING:" + lenghtstr + "}", randomstring);
                }
                else
                {
                    throw new FormatException("Invalid RANDSTRING format. Enter as {RANDSTRING:n} where n is length of sequence.");
                }
            }
            return format;
        }

        private string ParseFormatSEQNUM(string format, string seed)
        {
            var validseqnum = false;
            try
            {
                if (!format.Contains("{SEQNUM:") || !format.Contains("}"))
                {

                    return format;

                }
                var lenghtstr = format.Split(new string[] { "{SEQNUM:" }, StringSplitOptions.None)[1];
                lenghtstr = lenghtstr.Split('}')[0];
                if (int.TryParse(lenghtstr, out int length))
                {
                    if (length < 1)
                    {
                        throw new FormatException("SEQNUM length must be 1 or higher.");
                    }
                    var seedno = string.IsNullOrEmpty(seed) ? 1 : Int64.Parse(seed);
                    var sequence = string.Format("{0:" + new string('0', length) + "}", seedno);
                    format = format.Replace("{SEQNUM:" + lenghtstr + "}", sequence);
                    validseqnum = true;
                }
                else
                {
                    throw new FormatException("Invalid SEQNUM format. Enter as {SEQNUM:n} where n is length of sequence.");
                }
                if (format.Contains("{SEQNUM:"))
                {
                    throw new FormatException("Format string must only contain one {SEQNUM:n} placeholder.");
                }
            }
            finally
            {
                //txtSeed.Enabled = validseqnum;
                //btnGuessSeed.Enabled = validseqnum && !txtLogicalName.Enabled;
            }
            return format;
        }

        public void OnIncomingMessage(MessageBusEventArgs message)
        {
            throw new NotImplementedException();
        }

        #endregion Auto Number Format Methods

        #region Interface Members

        public string RepositoryName => "AutoNumberUpdater";

        public string UserName => "contactmayankp";
        
        public string DonationDescription => "Auto Number Updater";
        public string EmailAccount => "mayank.pujara@gmail.com";

        public string HelpUrl => "https://mayankp.wordpress.com/2021/12/09/xrmtoolbox-autonumberupdater-new-tool/";


        #endregion


        public void ShowAboutDialog()
        {
           // throw new NotImplementedException();
        }

        private void tslAbout_Click(object sender, EventArgs e)
        {
            Process.Start(HelpUrl);
        }

    }
}