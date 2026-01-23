using Newtonsoft.Json;

namespace Tomeshelf.MCM.Api.Responses;

/// <summary>
///     Represents the response data for an event, including its identifier, name, slug, and associated people.
/// </summary>
public class McmEventResponse
{
    /// <summary>
    ///     Gets or sets the unique identifier for the event.
    /// </summary>
    [JsonProperty("event_id")]
    public string EventId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the event associated with this instance.
    /// </summary>
    [JsonProperty("event_name")]
    public string EventName { get; set; }

    /// <summary>
    ///     Gets or sets the unique slug identifier for the event.
    /// </summary>
    [JsonProperty("event_slug")]
    public string EventSlug { get; set; }

    /// <summary>
    ///     Gets or sets the collection of people associated with this object.
    /// </summary>
    [JsonProperty("people")]
    public Person[] People { get; set; }

    /// <summary>
    ///     Represents a person or public figure, including personal details, social media profiles, and event-related
    ///     information.
    /// </summary>
    /// <remarks>
    ///     The Person class is used to model individuals with a variety of attributes, such as names,
    ///     biographies, social media handles, and event participation details. It is suitable for scenarios where
    ///     comprehensive
    ///     information about a person, including their online presence and event-related data, needs to be managed or
    ///     displayed. Many properties correspond to external identifiers or URLs for integration with third-party platforms.
    ///     Some properties, such as PeopleCategories and GlobalCategories, may reference other domain-specific types. Thread
    ///     safety is not guaranteed; if instances are shared across threads, callers should implement their own
    ///     synchronization.
    /// </remarks>
    public class Person
    {
        /// <summary>
        ///     Gets or sets the unique identifier for the object.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the unique identifier associated with this instance.
        /// </summary>
        [JsonProperty("uid")]
        public string Uid { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the item is visible to the public.
        /// </summary>
        [JsonProperty("publicly_visible")]
        public bool PubliclyVisible { get; set; }

        /// <summary>
        ///     Gets or sets the first name of the person.
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        ///     Gets or sets the last name of the person.
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        /// <summary>
        ///     Gets or sets the alternative name associated with the object.
        /// </summary>
        [JsonProperty("alt_name")]
        public string AltName { get; set; }

        /// <summary>
        ///     Gets or sets the user's biography or personal description.
        /// </summary>
        [JsonProperty("bio")]
        public string Bio { get; set; }

        /// <summary>
        ///     Gets or sets a description of the person's notable works or areas of recognition.
        /// </summary>
        [JsonProperty("known_for")]
        public string KnownFor { get; set; }

        /// <summary>
        ///     Gets or sets the URL of the user's profile.
        /// </summary>
        [JsonProperty("profile_url")]
        public string ProfileUrl { get; set; }

        /// <summary>
        ///     Gets or sets the display label for the user's profile URL.
        /// </summary>
        [JsonProperty("profile_url_label")]
        public string ProfileUrlLabel { get; set; }

        /// <summary>
        ///     Gets or sets the URL of the associated video.
        /// </summary>
        [JsonProperty("video_link")]
        public string VideoLink { get; set; }

        /// <summary>
        ///     Gets or sets the Twitter handle associated with the user or entity.
        /// </summary>
        [JsonProperty("twitter")]
        public string Twitter { get; set; }

        /// <summary>
        ///     Gets or sets the Facebook profile URL associated with the user.
        /// </summary>
        [JsonProperty("facebook")]
        public string Facebook { get; set; }

        /// <summary>
        ///     Gets or sets the Instagram handle or profile URL associated with the entity.
        /// </summary>
        [JsonProperty("instagram")]
        public string Instagram { get; set; }

        /// <summary>
        ///     Gets or sets the IMDb identifier associated with this item.
        /// </summary>
        [JsonProperty("imdb")]
        public string Imdb { get; set; }

        /// <summary>
        ///     Gets or sets the YouTube channel or video identifier associated with this entity.
        /// </summary>
        [JsonProperty("youtube")]
        public string YouTube { get; set; }

        /// <summary>
        ///     Gets or sets the Twitch username associated with this entity.
        /// </summary>
        [JsonProperty("twitch")]
        public string Twitch { get; set; }

        /// <summary>
        ///     Gets or sets the Snapchat username associated with the user or entity.
        /// </summary>
        [JsonProperty("snapchat")]
        public string Snapchat { get; set; }

        /// <summary>
        ///     Gets or sets the DeviantArt profile URL associated with the user.
        /// </summary>
        [JsonProperty("deviantart")]
        public string DeviantArt { get; set; }

        /// <summary>
        ///     Gets or sets the Tumblr username or URL associated with the entity.
        /// </summary>
        [JsonProperty("tumblr")]
        public string Tumblr { get; set; }

        /// <summary>
        ///     Gets or sets the name of the fandom associated with the item.
        /// </summary>
        [JsonProperty("fandom")]
        public string Fandom { get; set; }

        /// <summary>
        ///     Gets or sets the TikTok username or profile identifier associated with the entity.
        /// </summary>
        [JsonProperty("tiktok")]
        public string TikTok { get; set; }

        /// <summary>
        ///     Gets or sets the category associated with the item.
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>
        ///     Gets or sets the number of days the item has been listed as available for show.
        /// </summary>
        [JsonProperty("days_at_show")]
        public string DaysAtShow { get; set; }

        /// <summary>
        ///     Gets or sets the booth number associated with this entity.
        /// </summary>
        [JsonProperty("booth_number")]
        public string BoothNumber { get; set; }

        /// <summary>
        ///     Gets or sets the amount associated with the autograph transaction.
        /// </summary>
        [JsonProperty("autograph_amount")]
        public string AutographAmount { get; set; }

        /// <summary>
        ///     Gets or sets the amount associated with the photo opportunity, as a string.
        /// </summary>
        [JsonProperty("photo_op_amount")]
        public string PhotoOpAmount { get; set; }

        /// <summary>
        ///     Gets or sets the amount associated with the photo op table.
        /// </summary>
        [JsonProperty("photo_op_table_amount")]
        public string PhotoOpTableAmount { get; set; }

        /// <summary>
        ///     Gets or sets the collection of people categories associated with this object.
        /// </summary>
        [JsonProperty("people_categories")]
        public object[] PeopleCategories { get; set; }

        /// <summary>
        ///     Gets or sets the collection of global categories available in the system.
        /// </summary>
        [JsonProperty("global_categories")]
        public GlobalCategory[] GlobalCategories { get; set; }

        /// <summary>
        ///     Gets or sets the collection of images associated with this object.
        /// </summary>
        [JsonProperty("images")]
        public Image[] Images { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the resource is discoverable by others.
        /// </summary>
        [JsonProperty("discoverable")]
        public bool? Discoverable { get; set; }

        /// <summary>
        ///     Gets or sets the URL of the associated EPIC photo.
        /// </summary>
        [JsonProperty("epic_photo_url")]
        public string EpicPhotoUrl { get; set; }

        /// <summary>
        ///     Gets or sets the sort criteria to apply when retrieving results.
        /// </summary>
        /// <remarks>
        ///     The value can be a string, an array, or an object, depending on the requirements of the
        ///     consuming API. Refer to the API documentation for supported sort formats and usage examples.
        /// </remarks>
        [JsonProperty("sort")]
        public object Sort { get; set; }
    }

    /// <summary>
    ///     Represents a globally defined category with an identifier, name, and associated colour.
    /// </summary>
    public class GlobalCategory
    {
        /// <summary>
        ///     Gets or sets the unique identifier for the object.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the name associated with the object.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the colour associated with the object.
        /// </summary>
        [JsonProperty("color")]
        public string Colour { get; set; }
    }

    /// <summary>
    ///     Represents a set of image URLs in various sizes, typically used to provide different resolutions or formats for an
    ///     item.
    /// </summary>
    /// <remarks>
    ///     The Image class is commonly used to deserialize JSON objects containing multiple image
    ///     representations, such as large, medium, small, and thumbnail versions. Each property corresponds to a specific
    ///     image
    ///     size and may contain a URL or path to the image resource. Property values may be null or empty if a particular
    ///     image
    ///     size is not available.
    /// </remarks>
    public class Image
    {
        /// <summary>
        ///     Gets or sets the value associated with the "big" property in the JSON payload.
        /// </summary>
        [JsonProperty("big")]
        public string Big { get; set; }

        /// <summary>
        ///     Gets or sets the medium value associated with this object.
        /// </summary>
        [JsonProperty("med")]
        public string Med { get; set; }

        /// <summary>
        ///     Gets or sets the URL of the small-sized image associated with this object.
        /// </summary>
        [JsonProperty("small")]
        public string Small { get; set; }

        /// <summary>
        ///     Gets or sets the URL of the thumbnail image associated with the item.
        /// </summary>
        [JsonProperty("thumb")]
        public string Thumb { get; set; }
    }
}