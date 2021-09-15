package com.example.newestexperiment;

import static com.example.newestexperiment.MainActivity.defaultData;
import static com.example.newestexperiment.MainActivity.totalTime;
import static com.example.newestexperiment.MainActivity.userAns;
import static com.example.newestexperiment.MainActivity.verifyStoragePermissions;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.graphics.Bitmap;
import android.hardware.Camera;
import android.net.Uri;
import android.os.Bundle;
import android.provider.MediaStore;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class ExperimentA extends AppCompatActivity {
    public static long startTime;
    public static long lastCurrentTime;
    // 0 -> short unfamiliar; 1 -> long unfamiliar 2 -> short familiar 3 -> long familiar
    public static long[] totalTimeUnderOccasions = new long[4];
    static int ID = 0;

    private Camera mCamera;
    private CameraPreview mPreview;
    private ImageView imageView;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_experiment_a);
        startTime = System.currentTimeMillis();
        lastCurrentTime = startTime;
        verifyStoragePermissions(this);

//        defaultData.add(new Data("/storage/emulated/0/Pictures/Weixin/test.jpg",
//                new String[]{"候选项A","候选项B", "候选项C", "候选项D", "候选项E"}));
//
//        defaultData.add(new Data("/storage/emulated/0/Pictures/Weixin/test.jpg",
//                new String[]{"候选项N","候选项B", "候选项C", "候选项D", "候选项E"}));
//
//        defaultData.add(new Data("/storage/emulated/0/Pictures/Weixin/test.jpg",
//                new String[]{"候选项X","候选项B", "候选项C", "候选项D", "候选项E"}));
        addCaremaView();
        setData(ID);
    }
    public void addCaremaView() {
        mCamera = getCameraInstance();

        // Create our Preview view and set it as the content of our activity.
        mPreview = new CameraPreview(this, mCamera);
        mPreview.setOnClickListener((View v) -> {
            Log.e("click", "clickclick");
            ((ViewGroup)mPreview.getParent()).removeView(mPreview);
            addImageView();
        });
        FrameLayout preview = (FrameLayout) findViewById(R.id.camera_preview);
        preview.addView(mPreview);
    }

    public void addImageView() {
       imageView = new ImageView(this);
     //   Log.i("trail", this.getResources().getIdentifier("a2", "drawable", this.getPackageName()) + "");
        imageView.setImageResource(this.getResources().getIdentifier(
                "a" + (userAns.size() + 1), "drawable", this.getPackageName()));
        imageView.setOnClickListener((View v) -> {
            ((ViewGroup)imageView.getParent()).removeView(imageView);
            addCaremaView();
        });

        FrameLayout preview = (FrameLayout) findViewById(R.id.camera_preview);
        preview.addView(imageView);
    }

    public static Map<Integer, Integer> idMap = new HashMap<>();
    public static Map<Integer, Integer> mapId = new HashMap<>();
    public void setData(int currId) {
        //setBitMap(currId);
        LinearLayout linearLayout = findViewById(R.id.linear_layout);
        for(int i = 0;  i <= 5; i ++) {
            Button btn = new Button(this);
            if(i == 5) {
                btn.setText("以上都不是");
            } else {
                btn.setText(defaultData.get(currId).text[i]);
            }

            int tempId = View.generateViewId();
            Log.i("id", tempId + "");
            idMap.put(tempId, i);
            mapId.put(i, tempId);
            btn.setId(tempId);
            btn.setHeight(dpToPx(15));
            btn.setTextSize(22);
            LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.MATCH_PARENT, 200);
            params.setMargins(0, 2, 0, 0);
            btn.setLayoutParams(params);

            btn.setOnClickListener((View v) -> {
                long currTime =  System.currentTimeMillis();
                totalTimeUnderOccasions[defaultData.get(userAns.size()).length +
                        defaultData.get(userAns.size()).familiarity * 2] += currTime - lastCurrentTime;
                Log.i("tag", Arrays.toString(totalTimeUnderOccasions));
                Log.i("taggg", defaultData.get(userAns.size()).length +
                        defaultData.get(userAns.size()).familiarity * 2 +"");

                lastCurrentTime = currTime;

                int id = v.getId();
                userAns.add(idMap.get(id));
                Log.i("user answer: ", userAns.toString());
                changeButtonContent(userAns.size());
            });
            linearLayout.addView(btn);
        }
    }

    public void changeButtonContent(int id) {
        if(id >= defaultData.size()) {
            totalTime = System.currentTimeMillis() - startTime;
            Log.i("totalTime: ", totalTime + "");
            Intent intent = new Intent(this, Final.class);
            startActivity(intent);
        } else {
            for(int i = 0; i < 5; i++) {
                Button btn = findViewById(mapId.get(i));
                btn.setText(defaultData.get(id).text[i]);
            }
        }

        FrameLayout preview = (FrameLayout) findViewById(R.id.camera_preview);
        ((ViewGroup) preview).removeView(imageView);
        ((ViewGroup) preview).removeView(mPreview);
        addCaremaView();
    }

    private int dpToPx(int dp) {
        float density = getApplicationContext().getResources().getDisplayMetrics().density;
        return Math.round((float)dp * density);
    }

    public static Camera getCameraInstance(){
        Camera c = null;
        try {
            c = Camera.open(); // attempt to get a Camera instance
        }
        catch (Exception e){
            // Camera is not available (in use or does not exist)
        }
        return c; // returns null if camera is unavailable
    }

}