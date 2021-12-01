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
        private List<EntityMetadataProxy> entities;
        //private List<StringAttributeMetadata> attributeMetadata;
        private EntityMetadataProxy _selectedEntity;

        private AttributeProxy _selectedAttributeMetadata;
        //private string filteredEntity = "contact";
        //private string selectedAttribute = "cxm_contactreference";

        private bool _isPreviewOnly = true;

        //private string selectedFormat = "";
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

        private void FilterEntities()
        {
            cmbEntities.Items.Clear();
            
            var solution = cmbSolution.SelectedItem as SolutionProxy;
            if (solution == null)
            {
                return;
            }
            
            WorkAsync(new WorkAsyncInfo("Filtering entities...",
                (eventargs) =>
                {
                    cmbEntities.Enabled = false;
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
                            var filteredentities = entities.Where(e => includedentities.Entities.Select(i => i["objectid"]).Contains(e.Metadata.MetadataId));
                            cmbEntities.Items.AddRange(filteredentities.ToArray());
                        }
                    }
                    cmbEntities.Enabled = true;
                }
            });
        }

        private void LoadEntities()
        {
            //cmbEntities.Items.Clear();
            //cmbEntities.Enabled = false;
            entities = new List<EntityMetadataProxy>();
            WorkAsync(new WorkAsyncInfo("Loading entities...",
                (eventargs) =>
                {
                    //EnableControls(false);
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
                                .Select(m => new EntityMetadataProxy(m))
                                .OrderBy(e => e.ToString()));
                        }
                    }
                    //EnableControls(true);
                }
            });
        }

        //private void LoadEntities()
        //{
        //    entities = new List<EntityMetadata>();
        //    WorkAsync(new WorkAsyncInfo("Loading entities...",
        //        (eventargs) =>
        //        {
        //          eventargs.Result = MetadataHelper.LoadEntities(Service);
        //        })
        //    {
        //        PostWorkCallBack = (completedargs) =>
        //        {
        //            if (completedargs.Error != null)
        //            {
        //                MessageBox.Show(completedargs.Error.Message);
        //            }
        //            else
        //            {
        //                if (completedargs.Result is RetrieveMetadataChangesResponse)
        //                {
        //                    var metaresponse = ((RetrieveMetadataChangesResponse)completedargs.Result).EntityMetadata;
        //                    entities.AddRange(metaresponse
        //                        .Where(e => e.IsCustomizable.Value == true && e.IsIntersect.Value != true)
        //                        .Select(m=>m)
        //                        .OrderBy(e => e.ToString()));

        //                    ExecuteMethod(LoadAttributes);
        //                }
        //            }
                   
        //        }
        //    });
        //}

        //private void LoadAttributes()
        //{
        //    selectedEntity = entities.Find(a => a.Metadata.LogicalName.Equals(filteredEntity));
        //    var onlyNumbered = true;
        //    WorkAsync(new WorkAsyncInfo("Loading auto number attributes...",
        //        (eventargs) =>
        //        {
        //            if (selectedEntity.Metadata.Attributes == null)
        //            {
        //                eventargs.Result = MetadataHelper.LoadEntityDetails(Service, selectedEntity.Metadata.LogicalName).EntityMetadata.FirstOrDefault();
        //            }
        //            else
        //            {
        //                eventargs.Result = selectedEntity;
        //            }
        //        })
        //    {
        //        PostWorkCallBack = (completedargs) =>
        //        {
        //            if (completedargs.Result is EntityMetadata)
        //            {
        //                try
        //                {
        //                    selectedEntity.Metadata = (EntityMetadata)completedargs.Result;
        //                    var attributes = selectedEntity.Metadata.Attributes
        //                      .Where(a => a.AttributeType == AttributeTypeCode.String &&
        //                          a.IsValidForCreate.Value == true &&
        //                          a.IsCustomizable.Value == true &&
        //                          (!onlyNumbered || !string.IsNullOrEmpty(a.AutoNumberFormat)))
        //                      .Select(a => ((StringAttributeMetadata)a)).OrderBy(a => a.LogicalName).ToList();

        //                    attributeMetadata = attributes;


        //                    var attribute =
        //                        attributeMetadata.FirstOrDefault(a => a.LogicalName.Equals(selectedAttribute));

        //                    if (attribute != null)
        //                    {
        //                        selectedFormat = attribute.AutoNumberFormat;
        //                    }
        //                    ExecuteMethod(GetRecordsAndFixAutoNumbers);
        //                }
        //                catch (MissingMethodException mex)
        //                {
        //                    //LogUse("IncompatibleSDK");
        //                    MessageBox.Show("It seems you are using too old SDK, that is unaware of the AutoNumberFormat property.", "SDK error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                }
        //            }
        //        }
        //    });
        //}
        //private void GetAccounts()
        //{
        //    WorkAsync(new WorkAsyncInfo
        //    {
        //        Message = "Getting accounts",
        //        Work = (worker, args) =>
        //        {
        //            args.Result = Service.RetrieveMultiple(new QueryExpression("account")
        //            {
        //                TopCount = 50
        //            });
        //        },
        //        PostWorkCallBack = (args) =>
        //        {
        //            if (args.Error != null)
        //            {
        //                MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            }
        //            var result = args.Result as EntityCollection;
        //            if (result != null)
        //            {
        //                MessageBox.Show($"Found {result.Entities.Count} accounts");
        //            }

                    
        //        }
        //    });
        //}



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
            if (_isPreviewOnly)
            {
                LogTextBoxAndProgressBar.UpdateStatusMessage(StatusText, $"Running Under Preview Mode So NO records will be performed.");
            }
            else
            {
                LogTextBoxAndProgressBar.UpdateStatusMessage(StatusText, $"Running Under ACTUAL Mode So Records will be updated.");
            }

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
                            var primaryName = currentEntity.Contains(_selectedEntity.Metadata.PrimaryNameAttribute) ? currentEntity[_selectedEntity.Metadata.PrimaryNameAttribute].ToString():"";
                            UpdateStatusMessage($"Started Processing Id: {currentEntity.Id} and {_selectedEntity.Metadata.PrimaryNameAttribute}:{primaryName}.. ");


                            int currentLastValue = GuessSeed();
                            int nextValue = currentLastValue + 1;
                            var nextNumber = ParseNumberFormat(selectedFormat, nextValue.ToString());

                            if (!string.IsNullOrEmpty(nextNumber))
                            {
                                Entity updateEntity = new Entity(currentEntity.LogicalName);
                                updateEntity.Id = currentEntity.Id;
                                updateEntity[selectedAttribute] = nextNumber;


                                //only update if this not running under actual mode
                                if (!_isPreviewOnly)
                                {
                                    UpdateStatusMessage($" Record Id: {currentEntity.Id} and {_selectedEntity.Metadata.PrimaryNameAttribute}:{primaryName} will be updated with value  {selectedAttribute}:{nextNumber}");

                                    Service.Update(updateEntity);
                                }
                                else
                                {
                                    UpdateStatusMessage($"(PREVIEW) Record Id: {currentEntity.Id} and {_selectedEntity.Metadata.PrimaryNameAttribute}:{primaryName} will be updated with value  {selectedAttribute}:{nextNumber}");

                                }
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

                    //set preview mode to true after running!
                    _isPreviewOnly = true;
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
            var selectedFormat = "";
            
            var selectedAttribute = _selectedAttributeMetadata.LogicalName;
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
            //var entity = selectedEntity;
            var entity = entities.Find(a => a.Metadata.LogicalName.Equals(_selectedEntity.Metadata.LogicalName));
            var attributename = selectedAttribute;
            var fetchxml = "<fetch top='1' ><entity name='" + entity.Metadata.LogicalName + "' >" +
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
                            //UpdateUI(() =>
                            //{
                            //    gridAttributes.DataSource = source;
                            //    gridAttributes.Enabled = true;
                            //    gridAttributes.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                            //});
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

        private void LoadSolutions()
        {
            cmbSolution.Items.Clear();
            cmbSolution.Enabled = false;
            WorkAsync(new WorkAsyncInfo("Loading solutions...",
                (eventargs) =>
                {
                    //EnableControls(false);
                    var qx = new QueryExpression("solution");
                    qx.ColumnSet.AddColumns("friendlyname", "uniquename");
                    qx.AddOrder("installedon", OrderType.Ascending);
                    qx.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
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
                    //EnableControls(true);
                }
            });
        }

        private void cmbSolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterEntities();
        }

        private void cmbEntities_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedEntity = (EntityMetadataProxy) cmbEntities.SelectedItem;
            
            LoadAttributes(false);
        }

        private void cmbAttributes_SelectedIndexChanged(object sender, EventArgs e)
        {

            _selectedAttributeMetadata = (AttributeProxy)cmbAttributes.SelectedItem;

         
            if (_selectedAttributeMetadata != null)
            {
                var selectedFormat = _selectedAttributeMetadata.attributeMetadata.AutoNumberFormat;
                int currentLastValue = GuessSeed();
                int nextValue = currentLastValue + 1;
                txtSample.Text = ParseNumberFormat(selectedFormat, nextValue.ToString());

            }



        }

        private void btnAutoNumberPreview_Click(object sender, EventArgs e)
        {
            _isPreviewOnly = true;
            ExecuteMethod(GetRecordsAndFixAutoNumbers);
        }

        private void btnFixAutoNumbers_Click(object sender, EventArgs e)
        {
            _isPreviewOnly = false;
            ExecuteMethod(GetRecordsAndFixAutoNumbers);

        }
    }
}