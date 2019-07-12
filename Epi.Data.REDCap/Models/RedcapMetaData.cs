namespace Redcap.Models
{
    /// <summary>
    /// MetaData, the data dictionary for a particular variable in redcap
    /// </summary>
    public class RedcapMetaData
    {
        /// <summary>
        /// field name
        /// </summary>
        public string field_name { get; set; }
        /// <summary>
        /// the form name / instrument name
        /// </summary>
        public string form_name { get; set; }
        /// <summary>
        /// section header of instrument / form
        /// </summary>
        public string section_header { get; set; }
        /// <summary>
        /// the type of field, i.e "text", "radio"
        /// </summary>
        public string field_type { get; set; }
        /// <summary>
        /// the label for the field, "first name"
        /// </summary>
        public string field_label { get; set; }
        /// <summary>
        /// Choices, Calculations, OR Slider Labels
        /// </summary>
        public string select_choices_or_calculations { get; set; }
        /// <summary>
        /// field not, special instruction for the label
        /// </summary>
        public string field_note { get; set; }
        /// <summary>
        /// ui field validation
        /// </summary>
        public string text_validation_type_or_show_slider_number { get; set; }
        /// <summary>
        /// min value if int/num
        /// </summary>
        public string text_validation_min { get; set; }
        /// <summary>
        /// max value if int/num
        /// </summary>
        public string text_validation_max { get; set; }
        /// <summary>
        /// flag
        /// </summary>
        public string identifier { get; set; }
        /// <summary>
        /// the branching logic
        /// </summary>
        public string branching_logic { get; set; }
        /// <summary>
        /// flag
        /// </summary>
        public string required_field { get; set; }
        /// <summary>
        /// LH, RH etc
        /// </summary>
        public string custom_alignment { get; set; }
        /// <summary>
        /// if numbered
        /// </summary>
        public string question_number { get; set; }
        /// <summary>
        /// is this a matrix group question, what is the name of the group
        /// </summary>
        public string matrix_group_name { get; set; }
        /// <summary>
        /// the rank
        /// </summary>
        public string maxtrix_ranking { get; set; }
        /// <summary>
        /// field annotation
        /// </summary>
        public string field_annotation { get; set; }

    }

}
