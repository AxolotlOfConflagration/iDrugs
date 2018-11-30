﻿using AutoMapper;
using Common.Utils;
using DAL;
using DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.MedicineModels
{
    public class MedicineDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nazwa")]
        public string Name { get; set; }

        [Display(Name = "Cena całkowita")]
        public decimal UnitPrice { get; set; }

        //[Display(Name = "Refundacja")]
        public decimal? Refund { get; set; }

        [Display(Name = "Refundacja")]
        public string RefundString { get => Refund.HasValue ? $"{decimal.Round(Refund.Value * 100, 2)}%" : "Brak refundacji"; }

        [Display(Name = "Cena z refundacją")]
        public decimal PriceWithRefund
        {
            get
            {
                if (!Refund.HasValue) return UnitPrice;

                var price = decimal.Round(UnitPrice * (1 - Refund.Value), 2);
                return price;
            }
        }

        [Display(Name = "Ilość leku w opakowaniu")]
        public int Amount { get; set; }

        [Display(Name = "Jednostka")]
        public Unit Unit { get; set; }

        [Display(Name = "Rodzaj leku")]
        public MedType MedType { get; set; }

        [Display(Name = "Wycofany z użycia")]
        public bool Expired { get; set; }

        [Display(Name = "Zawartość opakowania")]
        public string MedicineContent { get => $"{MedType.GetDisplayName()} - {Amount} {Unit.AsPlural(Amount)}"; }


    }
}