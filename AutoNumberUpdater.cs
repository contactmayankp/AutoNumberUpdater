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
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using XrmToolBox.Extensibility;

namespace AutoNumberUpdater
{
    public partial class AutoNumberUpdater : PluginControlBase
    {
        private Settings mySettings;
        private List<EntityMetadata> entities;
        private List<StringAttributeMetadata> attributeMetadata;
        private EntityMetadata selectedEntity;
        private string filteredEntity = "contact";
        private string selectedAttribute = "cxm_contactreference";

        private string selectedFormat = "";
        public AutoNumberUpdater()
        {
            InitializeComponent();
        }

        private void AutoNumberUpdater_Load(object sender, EventArgs e)
        {
           // ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

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

        private void tsbFixAutoNumber_Click(object sender, EventArgs e)
        {
            // The ExecuteMethod method handles connecting to an
            // organization if XrmToolBox is not yet connected
            ExecuteMethod(LoadEntities);
            //ExecuteMethod(LoadAttributes);
            //ExecuteMethod(GetContacts);
            //ExecuteMethod(GetRecordsAndFixAutoNumbers);
        }


        private void LoadEntities()
        {
            entities = new List<EntityMetadata>();
            WorkAsync(new WorkAsyncInfo("Loading entities...",
                (eventargs) =>
                {
                  eventargs.Result = MetadataHelper.LoadEntities(Service);
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
                        if (completedargs.Result is RetrieveMetadataChangesResponse)
                        {
                            var metaresponse = ((RetrieveMetadataChangesResponse)completedargs.Result).EntityMetadata;
                            entities.AddRange(metaresponse
                                .Where(e => e.IsCustomizable.Value == true && e.IsIntersect.Value != true)
                                .Select(m=>m)
                                .OrderBy(e => e.ToString()));

                            ExecuteMethod(LoadAttributes);
                        }
                    }
                   
                }
            });
        }

        private void LoadAttributes()
        {
            selectedEntity = entities.Find(a => a.LogicalName.Equals(filteredEntity));
            var onlyNumbered = true;
            WorkAsync(new WorkAsyncInfo("Loading auto number attributes...",
                (eventargs) =>
                {
                    if (selectedEntity.Attributes == null)
                    {
                        eventargs.Result = MetadataHelper.LoadEntityDetails(Service, selectedEntity.LogicalName).EntityMetadata.FirstOrDefault();
                    }
                    else
                    {
                        eventargs.Result = selectedEntity;
                    }
                })
            {
                PostWorkCallBack = (completedargs) =>
                {
                    if (completedargs.Result is EntityMetadata)
                    {
                        try
                        {
                            selectedEntity = (EntityMetadata)completedargs.Result;
                            var attributes = selectedEntity.Attributes
                              .Where(a => a.AttributeType == AttributeTypeCode.String &&
                                  a.IsValidForCreate.Value == true &&
                                  a.IsCustomizable.Value == true &&
                                  (!onlyNumbered || !string.IsNullOrEmpty(a.AutoNumberFormat)))
                              .Select(a => ((StringAttributeMetadata)a)).OrderBy(a => a.LogicalName).ToList();

                            attributeMetadata = attributes;


                            var attribute =
                                attributeMetadata.FirstOrDefault(a => a.LogicalName.Equals(selectedAttribute));

                            if (attribute != null)
                            {
                                selectedFormat = attribute.AutoNumberFormat;
                            }
                            ExecuteMethod(GetRecordsAndFixAutoNumbers);
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
        private void GetAccounts()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting accounts",
                Work = (worker, args) =>
                {
                    args.Result = Service.RetrieveMultiple(new QueryExpression("account")
                    {
                        TopCount = 50
                    });
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
                        MessageBox.Show($"Found {result.Entities.Count} accounts");
                    }

                    
                }
            });
        }



        private void GetContacts()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting contacts",
                Work = (worker, args) =>
                {
                    
                   var result = GetAllContacts();
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
                        LogTextBoxAndProgressBar.UpdateStatusMessage(StatusText ,$"Found {result.Entities.Count} contacts..");
                    }

                    if (result != null)
                        LogTextBoxAndProgressBar.SetProgressBar(progressBar, result.Entities.Count);


                    FixContactAutoNumber(result);
                }
            });
        }


        private void GetRecordsAndFixAutoNumbers()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting " + selectedEntity.LogicalName + " Records which are missing auto numbers..",
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
                        LogTextBoxAndProgressBar.UpdateStatusMessage(StatusText, $"Found {result.Entities.Count} contacts..");
                    }

                    if (result != null)
                        LogTextBoxAndProgressBar.SetProgressBar(progressBar, result.Entities.Count);


                    FixEntityAutoNumbers(result);
                }
            });
        }
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

        private void FixEntityAutoNumbers(EntityCollection results)
        {

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Started Updating Auto Numbers for " + selectedEntity.LogicalName + " Records which are missing auto numbers..",
                Work = (worker, args) =>
                {
                    UpdateStatusMessage("Started Updating Auto Numbers for " + selectedEntity.LogicalName + " Records which are missing auto numbers..");


                    if (!String.IsNullOrEmpty(selectedFormat))
                    {

                        foreach (var currentEntity in results.Entities)
                        {
                            var primaryName = currentEntity.Contains(selectedEntity.PrimaryNameAttribute) ? currentEntity[selectedEntity.PrimaryNameAttribute].ToString():"";
                            UpdateStatusMessage($"Started Processing Id: {currentEntity.Id} and {selectedEntity.PrimaryNameAttribute}:{primaryName}.. ");


                            int currentLastValue = GuessSeed();
                            int nextValue = currentLastValue + 1;
                            var nextNumber = ParseNumberFormat(selectedFormat, nextValue.ToString());

                            if (!string.IsNullOrEmpty(nextNumber))
                            {
                                Entity updateEntity = new Entity(currentEntity.LogicalName);
                                updateEntity.Id = currentEntity.Id;
                                updateEntity[selectedAttribute] = nextNumber;

                                Service.Update(updateEntity);
                            }
                            else
                            {

                                UpdateStatusMessage($"Failed to Find Next number for Processing...");
                                break;
                            }

                            UpdateStatusMessage($"Completed Processing Id: {currentEntity.Id}");
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
                    UpdateStatusMessage("Finish Updating Auto Numbers for " + selectedEntity.LogicalName + " Records which are missing auto numbers..");

                },
                IsCancelable = true
            });
            
        }

        private EntityCollection GetAllRecordsWithOutAutoNumberPopulated()
        {
            var results = new EntityCollection();
             
            var query = new QueryExpression(selectedEntity.LogicalName)
            {
                ColumnSet = new ColumnSet(selectedEntity.LogicalName + "id", selectedEntity.PrimaryNameAttribute, selectedAttribute),
                PageInfo = new PagingInfo
                {
                    PageNumber = 1,
                    PagingCookie = null,
                    Count = 0x1388
                },
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition(selectedAttribute, ConditionOperator.Null);
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

        private EntityCollection GetAllContacts()
        {
            var results = new EntityCollection();
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("contactid", "fullname", "cxm_contactreference"),
                PageInfo = new PagingInfo
                {
                    PageNumber = 1,
                    PagingCookie = null,
                    Count = 0x1388
                },
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("cxm_contactreference", ConditionOperator.Null);
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


       

        private int GuessSeed()
        {
           
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
            //var entity = selectedEntity;
            var entity = entities.Find(a => a.LogicalName.Equals(selectedEntity.LogicalName));
            var attributename = selectedAttribute;
            var fetchxml = "<fetch top='1' ><entity name='" + entity.LogicalName + "' >" +
                "<attribute name='" + attributename + "' />" +
                "<filter><condition attribute='" + attributename + "' operator='not-null' /></filter>" +
                "<order attribute='" + selectedAttribute + "' descending='true' /></entity></fetch>";
            var lastrecord = Service.RetrieveMultiple(new FetchExpression(fetchxml)).Entities.FirstOrDefault();
            var result = 0;
            if (lastrecord == null)
            {
                //throw new Exception("No numbered data found for attribute " + attributename);
                return 0;
            }
            var lastvalue = lastrecord[attributename].ToString();
            if (lastvalue.Length >= seqstart + length)
            {
                var lastseqstr = lastvalue.Substring(seqstart, length);
                if (int.TryParse(lastseqstr, out int lastseq))
                {
                    //LogUse("GuessSeed succeeded");
                    result = lastseq;
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
                    //if (!chkAllowNoSeqNo.Checked)
                    //{
                    //    throw new FormatException("Format string must contain a {SEQNUM:n} placeholder.");
                    //}
                    //else
                    //{
                        return format;
                    //}
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

        private void FixContactAutoNumber(EntityCollection results)
        {
            string pattern = "NLC-{DATETIMEUTC:yyMMdd}-{SEQNUM:5}";
            
            foreach (var currentEntity in results.Entities)
            {
                LogTextBoxAndProgressBar.UpdateStatusMessage(StatusText, $"Started Processing Id: {currentEntity.Id}");

                var nextNumber = GetNextNumber();

                if (!string.IsNullOrEmpty(nextNumber))
                {
                    Entity updateEntity = new Entity(currentEntity.LogicalName);
                    updateEntity.Id = currentEntity.Id;
                    updateEntity["cxm_contactreference"] = nextNumber;

                    Service.Update(updateEntity);
                }
                else
                {

                    LogTextBoxAndProgressBar.UpdateStatusMessage(StatusText, $"Failed to Find Next number for Processing...");
                    break;
                }

                LogTextBoxAndProgressBar.UpdateStatusMessage(StatusText, $"Completed Processing Id: {currentEntity.Id}");
                LogTextBoxAndProgressBar.AddProgressStep(progressBar);
            }
        }


        private string GetNextNumber()
        {
            string nextNumber = "";
            int startValue = 1;
            //NLC-211202-01004
            //NLC - 211201 - 01003
            //NLC - 211114 - 01002
           //NLC - 211113 - 01001
            //NLC - 211112 - 01000

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                               <attribute name='contactid' />
                                <attribute name='cxm_contactreference' />
                                <order attribute='cxm_contactreference' descending='true' />
                                <filter type='and'>
                                  <condition attribute='cxm_contactreference' operator='not-null' />
                                </filter>
                              </entity>
                            </fetch>";

            var result = Service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result != null && result.Entities.Count > 0 && result.Entities[0].Contains("cxm_contactreference"))
            {
                var currentNumber = result.Entities[0]["cxm_contactreference"].ToString();

                int.TryParse(currentNumber.Substring(12), out var currentNumberInt);
                if (currentNumberInt != 0)
                {
                    var nextNumberInt = currentNumberInt + 1;
                    nextNumber = "NLC-" + DateTime.Now.ToString("yyMMdd") + "-" + nextNumberInt.ToString("D5");
                }


            }
            else
            {
                nextNumber = "NLC-" + DateTime.Now.ToString("yyMMdd") + "-" + startValue.ToString("D5");
            }

            return nextNumber;
        }


        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
        }
    }
}