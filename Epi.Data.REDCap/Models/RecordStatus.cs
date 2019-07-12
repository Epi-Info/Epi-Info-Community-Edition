using System.ComponentModel.DataAnnotations;

namespace Redcap.Models
{
    /// <summary>
    /// Represents the status of the record in redcap
    /// </summary>
    public enum RedcapStatus
    {
        /// <summary>
        /// Instrument is incomplete for the record
        /// </summary>
        [Display(Name ="Incomplete")]
        Incomplete = 0,
        /// <summary>
        /// Instrument is unverified for the record
        /// </summary>
        ///
        [Display(Name ="Unverified")]
        Unverified = 1,

        /// <summary>
        /// Instrument is complete for the record
        /// </summary>
        /// 
        [Display(Name ="Complete")]
        Complete = 2
    }
}