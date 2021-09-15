package com.example.newestexperiment;

import static com.example.newestexperiment.ExperimentA.getCameraInstance;
import static com.example.newestexperiment.MainActivity.defaultData;
import static com.example.newestexperiment.MainActivity.totalTime;
import static com.example.newestexperiment.MainActivity.userAns;

import androidx.annotation.ColorRes;
import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.graphics.Color;
import android.hardware.Camera;
import android.os.Bundle;
import android.os.Environment;
import android.provider.Settings;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Map;
import java.util.Random;
import java.util.Set;

public class ExperimentB extends AppCompatActivity {
    private Camera mCamera;
    private CameraPreview mPreview;
    private ImageView imageView;
    private static List<Integer> defaultImg;
    private static List<Integer> defaultSelections;
    private static int correctAns;
    private static Map<Integer, Integer> idMap;
    private static String[] strs =
            {"迈克尔杰克逊",
            "泰勒斯威夫特",
            "理查德布莱德曼",
            "安娜卡列尼娜",
            "乔治桑塔亚那",
            "折美恩度普瑞",
            "纳兹多拉奇翁",
            "费尔南多莫伦特",
            "列夫托尔斯泰",
            "陀思妥耶夫斯基",
            "马克西姆高尔基",
            "约翰施特劳斯",
            "格雷佛尔帕斯塔",
            "让弗朗索瓦商博良",
            "热比娅卡德尔",
            "尼拉吉秋普拉"};
    public static long startTime = 0;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_experiment_b);
        addCaremaView();
        setDefaultImg();
        addButtonText(0);
        addButtonListener();
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
        FrameLayout preview = (FrameLayout) findViewById(R.id.camera_preview2);
        preview.addView(mPreview);
    }

    public void addImageView() {
        imageView = new ImageView(this);
        //   Log.i("trail", this.getResources().getIdentifier("a2", "drawable", this.getPackageName()) + "");
        imageView.setImageResource(this.getResources().getIdentifier(
                "a" + (defaultImg.get(userAns.size())+1), "drawable", this.getPackageName()));
        imageView.setOnClickListener((View v) -> {
            ((ViewGroup)imageView.getParent()).removeView(imageView);
            addCaremaView();
        });

        FrameLayout preview = (FrameLayout) findViewById(R.id.camera_preview2);
        preview.addView(imageView);
    }
    private void setDefaultImg() {
        defaultImg = new ArrayList<>();
        defaultSelections = new ArrayList<>();
        for(Data d : defaultData) {
            if(d.length == 1 && (d.manipulation == 1 || d.manipulation == 3)) {
                defaultImg.add(d.id);
            }

            if(d.length == 1) {
                defaultSelections.add(d.id);
            }
        }
        //defaultImg.remove((Integer)57);
        //defaultImg.add(31);
    }

    private void addButtonListener() {
        List<View> allButtons;
        allButtons = findViewById(R.id.tableLayout2).getTouchables();
        for(View btn : allButtons) {
            if(btn instanceof Button) {
                setOnclick(btn);
                btn.setBackgroundColor(Color.GRAY);
                ((Button)btn).setTextSize(17);
            }
        }
        //Button btn = findViewById(R.id.button109);
        //setOnclick(btn);
    }

    private void setOnclick(View btn) {
        btn.setOnClickListener((View v) -> {
            int id = v.getId();
            if(userAns.isEmpty()) {
                startTime = System.currentTimeMillis();
            }
            if(idMap.get(id) == correctAns) {
                userAns.add(1);
            } else {
                userAns.add(0);
            }
            Log.i("userAns", userAns.toString());
            addButtonText(userAns.size());
        });
    }


    private void addButtonText(int i) {
        if(i >= 16) {
            totalTime = System.currentTimeMillis() - startTime;
            Intent intent = new Intent(this, Final.class);
            startActivity(intent);
        } else {
            List<View> allButtons;
            allButtons = findViewById(R.id.tableLayout2).getTouchables();
            List<Integer> buttonText = generateRandomNum();
            Log.i("Random num", buttonText.toString());
            Log.i("2333", defaultImg.toString());
            if(buttonText.contains(i)) {
                correctAns = buttonText.indexOf(i);
                //Log.i("correctAns", correctAns + "");
            } else {
                correctAns = -1;
            }
            int cnt = 0;
            idMap = new HashMap<>();
            for(View v : allButtons) {
                if(v instanceof Button) {
                    idMap.put(v.getId(), cnt);
                    //Log.i("buttonText", buttonText.get(cnt) + "");
                    ((Button)v).setText(getCorrectAnsStr(buttonText.get(cnt++)));
                }
            }
            ((Button)findViewById(R.id.button109)).setText("以上都不对");
            //Log.i("map: ", idMap.toString());
            idMap.put(R.id.button109, -1);

            FrameLayout preview = (FrameLayout) findViewById(R.id.camera_preview2);
            ((ViewGroup) preview).removeView(imageView);
            ((ViewGroup) preview).removeView(mPreview);
            addCaremaView();
        }


    }

    private String getCorrectAnsStr(int id) {
       // Log.i("id: ", id + " " + defaultData.get(id).correctAns);
        return defaultData.get(defaultSelections.get(id)).text[defaultData.get(defaultSelections.get(id)).correctAns - 1];
    }


    private List<Integer> generateRandomNum() {
        Random rng = new Random(); // Ideally just create one instance globally
        Set<Integer> generated = new LinkedHashSet<Integer>();
        while (generated.size() < 16)
        {
            Integer next = rng.nextInt(32);
            // As we're adding to a set, this will automatically do a containment check
            generated.add(next);
        }
        return new ArrayList<>(generated);
    }

}