package com.example.newestexperiment;

import java.util.Arrays;
import java.util.List;

public class Data {
   // public List<String> text;
    public int id;
    public String[] text;
    public int correctAns;
    // 1 -> unchanged
    // 2 -> blur
    // 3 -> distortion
    // 4 -> incomplete
    public int manipulation;
    // 0 -> short 1 -> long
    public int length;
    // 0 -> unfamiliar
    public int familiarity;
    public Data(String[] data, int correctAns, int manipulation) {
        text = data;
        this.correctAns = correctAns;
        this.manipulation = manipulation;
    }

    public Data(String[] data, int correctAns, int length, int familiarity) {
        text = data;
        this.correctAns = correctAns;
        this.length = length;
        this.familiarity = familiarity;
    }

    public Data(int id, String[] data, int correctAns, int familiarity, int length, int manipulation) {
        this.id = id;
        text = data;
        this.correctAns = correctAns;
        this.length = length;
        this.familiarity = familiarity;
        this.manipulation =  manipulation;
    }
    @Override
    public String toString() {
        return "Data{" +
                "text=" + Arrays.toString(text) +
                ", correctAns=" + correctAns +
                ", manipulation=" + manipulation +
                ", length=" + length +
                ", familiarity=" + familiarity +
                '}';
    }
}