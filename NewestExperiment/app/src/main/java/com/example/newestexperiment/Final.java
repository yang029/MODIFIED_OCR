package com.example.newestexperiment;



import static com.example.newestexperiment.ExperimentA.totalTimeUnderOccasions;
import static com.example.newestexperiment.MainActivity.totalTime;
import static com.example.newestexperiment.MainActivity.userAns;
import static com.example.newestexperiment.MainActivity.writeData;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.util.Log;

import java.io.IOException;
import java.util.Arrays;
import java.util.List;
import java.util.stream.Collectors;

public class Final extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_final);
        Log.i("final results: ", userAns.toString());
        String[] newAns = new String[userAns.size()];
        List<String> ansStr = userAns.stream().map(Object::toString)
                .collect(Collectors.toList());
        ansStr.add(0, totalTime + "");
        if(ansStr.size() == 65) {
            ansStr.add(0,"A");
            //Log.i("different time", Arrays.toString(totalTimeUnderOccasions));
            ansStr.addAll(Arrays.stream(totalTimeUnderOccasions).mapToObj(String::valueOf)
                    .collect(Collectors.toList()));
        } else {
            ansStr.add(0,"B");
        }
        try {
            writeData(ansStr.toArray(newAns));
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}