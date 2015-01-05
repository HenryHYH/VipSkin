﻿using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Nop.Plugin.Widgets.ProductDetailReview.Models
{
    public class PublicInfoModel : BaseNopModel
    {
        public PublicInfoModel()
        {
            Items = new List<ProductReviewModel>();
            AddProductReview = new AddProductReviewModel();
        }

        public IList<ProductReviewModel> Items { get; set; }

        public AddProductReviewModel AddProductReview { get; set; }

        public int ProductId { get; set; }

        public bool AllowCustomerReviews { get; set; }
    }

    public partial class ProductReviewModel : BaseNopEntityModel
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public bool AllowViewingProfiles { get; set; }
        
        public string Title { get; set; }

        public string ReviewText { get; set; }

        public int Rating { get; set; }

        public ProductReviewHelpfulnessModel Helpfulness { get; set; }

        public string WrittenOnStr { get; set; }
    }

    public partial class ProductReviewHelpfulnessModel : BaseNopModel
    {
        public int ProductReviewId { get; set; }

        public int HelpfulYesTotal { get; set; }

        public int HelpfulNoTotal { get; set; }
    }

    public partial class AddProductReviewModel : BaseNopModel
    {
        [AllowHtml]
        [NopResourceDisplayName("Reviews.Fields.Title")]
        public string Title { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Reviews.Fields.ReviewText")]
        public string ReviewText { get; set; }

        [NopResourceDisplayName("Reviews.Fields.Rating")]
        public int Rating { get; set; }

        public bool DisplayCaptcha { get; set; }

        public bool CanCurrentCustomerLeaveReview { get; set; }

        public bool SuccessfullyAdded { get; set; }

        public string Result { get; set; }
    }
}